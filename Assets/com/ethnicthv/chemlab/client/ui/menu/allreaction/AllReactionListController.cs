using System;
using System.Collections.Generic;
using System.Linq;
using com.ethnicthv.chemlab.engine.api.reaction;
using com.ethnicthv.chemlab.engine.reaction;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.ui.menu.allreaction
{
    public class AllReactionListController : MonoBehaviour
    {
        [SerializeField] private AllReactionItemController itemPrefab;
        [SerializeField] private Transform itemContainer;
        
        private readonly HashSet<string> _createdItems = new HashSet<string>();

        private void OnEnable()
        {
            Setup();
        }

        private void Setup()
        {
            var reactions = ReactingReaction.GetAllReactions();
            var temp = new List<IReactingReaction>(reactions);
            //Note: Sort the molecules by their ID
            temp.Sort((a, b) => string.Compare(a.GetId(), b.GetId(), StringComparison.Ordinal));

            foreach (var reaction in temp.Where(reaction => !_createdItems.Contains(reaction.GetId())))
            {
                CreateItem(reaction);
                _createdItems.Add(reaction.GetId());
            }
        }
        
        private void CreateItem(IReactingReaction reaction)
        {
            var item = Instantiate(itemPrefab, itemContainer);
            item.SetReaction(reaction);
            item.gameObject.SetActive(true);
        }
    }
}