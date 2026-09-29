using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    //when object enter or exit the trigger, put it to the assigned layer and sorting layers base on the direction
    //used in the stairs objects for player to travel between layers

    public class StairsLayerTrigger : MonoBehaviour
    {
        public Direction direction;                                  //direction of the stairs
        [Space]
        public string layerUpper;
        public string sortingLayerUpper;
        [Space]
        public string layerLower;
        public string sortingLayerLower;
        

        private void OnTriggerEnter2D(Collider2D other)
        {
            bool isAllowed = (other.gameObject.CompareTag("feet"));
            if (!isAllowed) { return; }
            if (other.attachedRigidbody == null) return;
            GameObject rootObject = other.attachedRigidbody.gameObject;
                if (direction == Direction.South && other.transform.position.y < transform.position.y && isAllowed) SetLayerAndSortingLayer(rootObject, layerUpper, sortingLayerUpper);
                else
                    if (direction == Direction.West && other.transform.position.x < transform.position.x && isAllowed) SetLayerAndSortingLayer(rootObject, layerUpper, sortingLayerUpper);
                    else
                        if (direction == Direction.East && other.transform.position.x > transform.position.x &&isAllowed) SetLayerAndSortingLayer(rootObject, layerUpper, sortingLayerUpper);
            
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            bool isAllowed = (other.gameObject.CompareTag("feet"));
            if (!isAllowed) { return; }
            if (other.attachedRigidbody == null) return;
            GameObject rootObject = other.attachedRigidbody.gameObject;

            if (direction == Direction.South && other.transform.position.y < transform.position.y &&isAllowed) SetLayerAndSortingLayer(rootObject, layerLower, sortingLayerLower);
                else
                    if (direction == Direction.West && other.transform.position.x < transform.position.x &&isAllowed) SetLayerAndSortingLayer(rootObject, layerLower, sortingLayerLower);
                    else
                        if (direction == Direction.East && other.transform.position.x > transform.position.x &&isAllowed) SetLayerAndSortingLayer(rootObject, layerLower, sortingLayerLower);
            
        }

        private void SetLayerAndSortingLayer(GameObject target, string layer, string sortingLayer)
        {

            if (!target.gameObject.CompareTag("PushableBox")) { 
            target.layer = LayerMask.NameToLayer(layer);
            Transform[] transforms = target.GetComponentsInChildren<Transform>(true);

            foreach (Transform transform in transforms)
            {
                transform.gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }

            target.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
            SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }
        }

        public enum Direction
        {
            North,
            South,
            West,
            East
        }    
    }
}
