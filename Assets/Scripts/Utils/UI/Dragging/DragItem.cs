using Inventories;
using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using Control;
using Attributes;

namespace Utils.UI.Dragging
{
    public class DragItem<T> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler where T : class
    {
        // CACHE

        Canvas parentCanvas;

        // STATE

        Vector3 startPosition;
        Transform originalParent;
        IDragSource<T> source;
        InventoryDropTarget dropArea;
        FollowCamera followCamera;
        FightScheduler fightScheduler;
        ControlSwitcher controlSwitcher;

        // LIFECYCLE

        void Awake()
        {
            parentCanvas = GetComponentInParent<Canvas>();
            source = GetComponentInParent<IDragSource<T>>();
        }

        void Start()
        {
            followCamera = FindObjectOfType<FollowCamera>();
            dropArea = FindObjectOfType<InventoryDropTarget>();
            fightScheduler = FindObjectOfType<FightScheduler>();
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
        }

        // PRIVATE

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            startPosition = transform.position;
            originalParent = transform.parent;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            transform.SetParent(parentCanvas.transform, true);

            followCamera.SetDragging(true);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            transform.position = eventData.position;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            transform.position = startPosition;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            transform.SetParent(originalParent, true);

            IDragDestination<T> container;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                container = dropArea.GetComponent<IDragDestination<T>>();

                if (controlSwitcher.GetActivePlayer().GetComponent<Health>().IsDead())
                {
                    container = null;
                }
            }
            else
            {
                container = GetContainer(eventData);
            }

            if (container != null)
            {
                DropItemIntoContainer(container);
            }

            followCamera.SetDragging(false);
        }

        IDragDestination<T> GetContainer(PointerEventData eventData)
        {
            if (eventData.pointerEnter)
            {
                var container = eventData.pointerEnter.GetComponentInParent<IDragDestination<T>>();

                return container;
            }
            return null;
        }

        void DropItemIntoContainer(IDragDestination<T> destination)
        {
            if (object.ReferenceEquals(destination, source)) return;

            var destinationContainer = destination as IDragContainer<T>;
            var sourceContainer = source as IDragContainer<T>;

            if (destinationContainer != null && sourceContainer != null && fightScheduler.IsFightRunning() &&
               (destinationContainer.GetSlotType() == SlotType.Equipment || sourceContainer.GetSlotType() == SlotType.Equipment))
            {
                return;
            }

            if (destinationContainer == null || sourceContainer == null || destinationContainer.GetItem() == null ||
               (object.ReferenceEquals(destinationContainer.GetItem(), sourceContainer.GetItem()) && destinationContainer.IsItemStackable()))
            {
                AttemptSimpleTransfer(destination);
                return;
            }

            AttemptSwap(destinationContainer, sourceContainer);
        }

        void AttemptSwap(IDragContainer<T> destination, IDragContainer<T> source)
        {
            var removedSourceNumber = source.GetNumber();
            var removedSourceItem = source.GetItem();
            var slotType = source.GetSlotType();
            var removedDestinationNumber = destination.GetNumber();
            var removedDestinationItem = destination.GetItem();

            source.RemoveItems(removedSourceNumber);
            destination.RemoveItems(removedDestinationNumber);

            var sourceTakeBackNumber = CalculateTakeBack(removedSourceItem, removedSourceNumber, source, destination);
            var destinationTakeBackNumber = CalculateTakeBack(removedDestinationItem, removedDestinationNumber, destination, source);

            if (sourceTakeBackNumber > 0)
            {
                source.AddItems(removedSourceItem, sourceTakeBackNumber);
                removedSourceNumber -= sourceTakeBackNumber;
            }
            if (destinationTakeBackNumber > 0)
            {
                destination.AddItems(removedDestinationItem, destinationTakeBackNumber);
                removedDestinationNumber -= destinationTakeBackNumber;
            }

            if (source.MaxAcceptable(removedDestinationItem, slotType) < removedDestinationNumber ||
                destination.MaxAcceptable(removedSourceItem, slotType) < removedSourceNumber)
            {
                destination.AddItems(removedDestinationItem, removedDestinationNumber);
                source.AddItems(removedSourceItem, removedSourceNumber);
                return;
            }

            if (removedDestinationNumber > 0)
            {
                source.AddItems(removedDestinationItem, removedDestinationNumber);
            }
            if (removedSourceNumber > 0)
            {
                destination.AddItems(removedSourceItem, removedSourceNumber);
            }
        }

        bool AttemptSimpleTransfer(IDragDestination<T> destination)
        {
            var draggingItem = source.GetItem();
            var draggingNumber = source.GetNumber();
            var slotType = source.GetSlotType();

            if (slotType == SlotType.SkillDeck)
            {
                destination.FlushItem();
                destination.AddItems(draggingItem, 1);
            }
            else
            {
                var acceptable = destination.MaxAcceptable(draggingItem, slotType);
                var toTransfer = Mathf.Min(acceptable, draggingNumber);

                if (toTransfer > 0)
                {
                    source.RemoveItems(toTransfer);
                    destination.AddItems(draggingItem, toTransfer);
                    return false;
                }
            }
            
            return true;
        }

        int CalculateTakeBack(T removedItem, int removedNumber, IDragContainer<T> removeSource, IDragContainer<T> destination)
        {
            var slotType = removeSource.GetSlotType();
            var takeBackNumber = 0;
            var destinationMaxAcceptable = destination.MaxAcceptable(removedItem, slotType);

            if (destinationMaxAcceptable < removedNumber)
            {
                takeBackNumber = removedNumber - destinationMaxAcceptable;

                var sourceTakeBackAcceptable = removeSource.MaxAcceptable(removedItem, slotType);

                if (sourceTakeBackAcceptable < takeBackNumber)
                {
                    return 0;
                }
            }
            return takeBackNumber;
        }
    }
}