using System;
using System.Collections;
using System.Collections.Generic;
using com.ethnicthv.assets.input;
using com.ethnicthv.chemlab.client.api.core.game;
using com.ethnicthv.chemlab.client.ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace com.ethnicthv.chemlab.client.core.game
{
    [RequireComponent(typeof(ClientManager))]
    public class GameRaycastManager : MonoBehaviour
    {
        public static GameRaycastManager Instance { get; private set; }

        [SerializeField] private float hoverMinimumTime = 0.5f;
        [SerializeField] private int skipFixedFrames = 5;
        [SerializeField] private float dragVelocity = 10;
        [SerializeField] private LayerMask draggableLayer;
        [SerializeField] private LayerMask tableLayer;
        
        public event Action<RaycastHit> OnRaycastHit = delegate { };

        private GameInteract _gameInteract;

        private bool _isLeftPointerOverGameObject;
        private bool _isRightPointerOverGameObject;

#if UNITY_EDITOR
        private readonly Queue<Vector3> _debugRaycastHits = new();
#endif

        private int _skipFrames;
        private GameObject _hitGameObject;
        private float _hoverInterval;

        private void Awake()
        {
            Instance = this;

            _gameInteract = new GameInteract();

            _gameInteract.GameEnvironment.Interact.performed += _ => OnLeftClick();
            _gameInteract.GameEnvironment.Options.performed += _ => OnRightClick();
            _gameInteract.GameEnvironment.Hold.performed += _ => OnDragStart();
            _gameInteract.GameEnvironment.Hold.canceled += _ => OnDragEnd();
            _gameInteract.GameEnvironment.MouseMove.performed += _ => OnMouseMove();

            _gameInteract.Enable();
        }

        private void Start()
        {
            _skipFrames = skipFixedFrames;
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _isLeftPointerOverGameObject = EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId);
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _isRightPointerOverGameObject = EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseRightId);
            }
        }

        private void FixedUpdate()
        {
            if (_skipFrames > 0)
            {
                _skipFrames--;
                return;
            }

            _skipFrames = skipFixedFrames;

            if (EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId)) return;
            var ray = ClientManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;

            OnRaycastHit.Invoke(hit);
            
            if (_hitGameObject != hit.collider.gameObject)
            {
                _hitGameObject = hit.collider.gameObject;
                _hoverInterval = 0;
            }
            else
            {
                _hoverInterval += Time.fixedDeltaTime;
            }
            
            if (_hoverInterval < hoverMinimumTime) return;
            
            if (!InteractableManager.TryGetInteractable(hit.collider.gameObject, out var interactable)) return;
            
            //Note: Hover Event
            interactable.OnHover();
            
            //Note: Open Hover Panel
            var hover = interactable.GetHoverPanel();
            if (hover.panelObject == null) return;
            var panel = UIManager.Instance.OpenHoverPanel(hover.panelObject);
            hover.setupFunction?.Invoke(panel);
        }

        private void OnMouseMove()
        {
            UIManager.Instance.CloseHoverPanel();
        }

        private void OnLeftClick()
        {
            //Note: Close the options panel if it is open
            UIManager.Instance.OptionsPanelController.ClosePanel();
            
            if (_isLeftPointerOverGameObject) return;

            var ray = ClientManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit)) return;

            if (InteractableManager.TryGetInteractable(hit.collider.gameObject, out var interactable))
            {
                interactable.OnInteract();
            }

#if UNITY_EDITOR
            _debugRaycastHits.Enqueue(hit.point);
            if (_debugRaycastHits.Count > 5)
            {
                _debugRaycastHits.Dequeue();
            }
#endif
        }

        private void OnRightClick()
        {
            if (_isRightPointerOverGameObject) return;

            var ray = ClientManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit)) return;

            if (InteractableManager.TryGetInteractable(hit.collider.gameObject, out var interactable))
            {
                OpenOptionsPanel(interactable);
            }

#if UNITY_EDITOR
            _debugRaycastHits.Enqueue(hit.point);
            if (_debugRaycastHits.Count > 5)
            {
                _debugRaycastHits.Dequeue();
            }
#endif
        }

        private void OpenOptionsPanel(IInteractable interactable)
        {
            var options = interactable.GetOptions();
            if (options == null || options.Count == 0) return;
            
            UIManager.Instance.OptionsPanelController.SetupOptions(options, Mouse.current.position.value);
            UIManager.Instance.OptionsPanelController.OpenPanel();
        }
        
        private void OpenDropOptionsPanel(IInteractable interactable, GameObject other)
        {
            var options = interactable.GetDropOptions(other);
            if (options == null) return;
            
            UIManager.Instance.OptionsPanelController.SetupOptions(options, Mouse.current.position.value);
            UIManager.Instance.OptionsPanelController.OpenPanel();
        }

        private Coroutine _dragCoroutine;
        private Vector3 _velocity = Vector3.zero;
        private IInteractable _dragInteractable;
        private GameObject _dragGameObject;
        private int _originalLayer;

        private void OnDragStart()
        {
            if (_isLeftPointerOverGameObject) return;

            var ray = ClientManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, maxDistance: Mathf.Infinity, layerMask: draggableLayer)) return;

            var colliderGameObject = hit.collider.gameObject;
            
            if (InstrumentManager.TryGetInstrument(colliderGameObject, out var instrument))
            {
                switch (instrument)
                {
                    case IHeater heater when heater.IsAttachedToHeatable(out _):
                    case IHeatable heatable when heatable.IsAttachedToHeater(out _):
                        return;
                }
            }
            
            if (!InteractableManager.TryGetInteractable(colliderGameObject, out var interactable)) return;
            
            //Note: change layer of the interactable to prevent raycast from hitting it to
            _originalLayer = colliderGameObject.layer;
            colliderGameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            
            _dragInteractable = interactable;
            _dragGameObject = colliderGameObject;
            _dragCoroutine = StartCoroutine(DragCoroutine(colliderGameObject, interactable));
        }

        private IEnumerator DragCoroutine(GameObject go, IInteractable interactable)
        {
            while (true)
            {
                if (EventSystem.current.IsPointerOverGameObject(PointerInputModule.kMouseLeftId))
                {
                    go.layer = _originalLayer;
                    _dragCoroutine = null;
                    OnDragEnd();
                    yield break;
                }
                
                var ray = ClientManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
                var interactableTransform = interactable.GetMainTransform();
                var newPosition = interactable.GetMainTransform().position;
                if (Physics.Raycast(ray, out var hit, maxDistance: Mathf.Infinity, layerMask: tableLayer))
                {
                    newPosition = hit.point;
                }

                interactableTransform.position = Vector3.SmoothDamp(interactableTransform.position, newPosition,
                    ref _velocity, 1 / dragVelocity);
                yield return null;
            }
        }

        private void OnDragEnd()
        {
            if (_dragCoroutine != null)
            {
                StopCoroutine(_dragCoroutine);
                var ray = ClientManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
                GameObject other = null;
                if (Physics.Raycast(ray, out var hit, maxDistance: Mathf.Infinity, layerMask: draggableLayer))
                {
                    other = hit.collider.gameObject;
                    OpenDropOptionsPanel(_dragInteractable, other);
                }
                _dragInteractable.OnDrop(other);
                _dragGameObject.layer = _originalLayer;
            }
            _dragInteractable = null;
            _dragGameObject = null;
            _dragCoroutine = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (_debugRaycastHits.Count < 2) return;
            foreach (var hit in _debugRaycastHits)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit, 0.1f);
            }
        }
#endif
    }
}