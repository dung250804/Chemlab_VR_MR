using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stream : MonoBehaviour
{
   [SerializeField]private LineRenderer lineRenderer;

   private Coroutine _pourCoroutine;
   private Vector3 _targetPosition = Vector3.zero;
   private const float MAX_LENGTH = 2f;

   void Start()
   {
      MoveToPosition(0, transform.position);
      MoveToPosition(1, transform.position);
   }

   public void Begin()
   {
      if (_pourCoroutine == null)
      {
         lineRenderer.enabled = true;
         MoveToPosition(0, transform.position);
         MoveToPosition(1, transform.position);
         _pourCoroutine = StartCoroutine(BeginPour());
      }
   }

   private IEnumerator BeginPour()
   {
      while(gameObject.activeSelf)
      {
         _targetPosition = FindEndPoint();
         MoveToPosition(0, transform.position);
         AnimateToPosition(1, _targetPosition);
         yield return null;  
      }
   }

   public void End(Action onFinish)
   {
      if (_pourCoroutine != null)
      {
         StopCoroutine(_pourCoroutine);
         _pourCoroutine = null;
      }
        
      _pourCoroutine = StartCoroutine(EndPour(onFinish));
   }

   private IEnumerator EndPour(Action onFinish)
   {
      while(!HasReachedPosition(0, _targetPosition))
      {
         AnimateToPosition(0, _targetPosition); 
         AnimateToPosition(1, _targetPosition); 
         yield return null;
      }
      lineRenderer.enabled = false;
      _pourCoroutine = null;
      onFinish?.Invoke();
   }

   private Vector3 FindEndPoint()
   {
      RaycastHit hit;
      Vector3 rayOrigin = transform.position + Vector3.down * 0.02f;
      Ray ray = new Ray(rayOrigin, Vector3.down);
      
      if (Physics.Raycast(ray, out hit, MAX_LENGTH))
      {
         while (transform.IsChildOf(hit.collider.transform))
         {
            rayOrigin += Vector3.down * 0.02f;
            Debug.Log(rayOrigin);
            ray = new Ray(rayOrigin, Vector3.down);
            if (!Physics.Raycast(ray, out hit, MAX_LENGTH))
               break;
         }
         Vector3 endPoint = hit.collider ? hit.point : ray.GetPoint(MAX_LENGTH);

         return endPoint;
      }
      return ray.GetPoint(MAX_LENGTH);
   }

   private void MoveToPosition(int index, Vector3 targetPosition)
   {
      lineRenderer.SetPosition(index, targetPosition);
   }

   private void AnimateToPosition(int index, Vector3 targetPosition)
   {
      Vector3 currentPoint = lineRenderer.GetPosition(index);
      Vector3 newPosition = Vector3.MoveTowards(currentPoint, targetPosition, Time.deltaTime * 2f);
      lineRenderer.SetPosition(index, newPosition);
   }

   private bool HasReachedPosition(int index, Vector3 targetPosition)
   {
      Vector3 current = lineRenderer.GetPosition(index);
      return Vector3.Distance(current, targetPosition) < 0.01f;
   }

   private IEnumerator UpdateParticle()
   {
      // while (particle.gameObject.activeSelf)
      // {
      //    particle.gameObject.transform.position = _targetPosition;

      //    bool isHitting = HasReachedPosition(1, _targetPosition);
      //    particle.gameObject.SetActive(isHitting);
      //    yield return null;
      // }
      yield return null;
   }

   public LineRenderer GetLineRenderer() => lineRenderer;
}
