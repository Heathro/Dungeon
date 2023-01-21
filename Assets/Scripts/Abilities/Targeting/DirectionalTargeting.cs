using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Control;
using Utils;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Directional", menuName = "Abilities/Targeting/Directional", order = 0)]
    public class DirectionalTargeting : TargetingStrategy
    {
        // CONFIG

        [SerializeField] CursorType cursorType = CursorType.Air;
        [SerializeField] LayerMask layerMask;
        [SerializeField] float groundOffset = 0.04336333f;
        [SerializeField] float sphereCastRadius = 1f;
        [SerializeField] float distanceTolerance = 0f;

        // PUBLIC

        public override void StartTargeting(AbilityData data, Action finished)
        {
            PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
            playerController.SetCursor(cursorType, true);
            playerController.SetTargeting(true);
            playerController.StartCoroutine(Targeting(data, playerController, finished));
        }

        IEnumerator Targeting(AbilityData data, PlayerController playerController, Action finished)
        {
            bool leftActionBar = false;
            bool targetAquired = false;

            Vector3 caster = data.GetUser().transform.position;
            Vector3 destination = data.GetUser().transform.position; 
            Vector3 direction = data.GetUser().transform.position;

            while (!data.IsCancelled() && !targetAquired)
            {
                playerController.SetCursor(cursorType, true);

                if (Input.GetKeyDown(KeyCode.Escape) || (leftActionBar && EventSystem.current.IsPointerOverGameObject()))
                {
                    playerController.EnableControl(true);
                    playerController.EnableTargetSystem(true);
                    playerController.SetTargeting(false);
                    data.CancelAction();
                    break;
                }
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    leftActionBar = true;
                }

                RaycastHit raycastHit;
                Ray ray = PlayerController.GetMouseRay();
                if (Physics.Raycast(ray, out raycastHit, 1000, layerMask))
                {
                    destination = raycastHit.point + Vector3.up * groundOffset;
                    data.SetTargetPoint(destination);

                    float distance = Vector3.Distance(caster, destination);

                    if (distance > data.GetDistance())
                    {
                        playerController.SetActionPointsDisplay(distance, "Out of Range");
                    }
                    else
                    {
                        playerController.SetActionPointsDisplay(distance, "In Range");
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        yield return new WaitUntil(() => Input.GetMouseButton(0));

                        direction = -(caster - destination).normalized;
                        float distance = data.GetDistance() - distanceTolerance;
                        RaycastHit[] hits = Physics.SphereCastAll(caster, sphereCastRadius, direction, distance);

                        List<GameObject> objectsFound = new List<GameObject>();
                        foreach (RaycastHit hit in hits)
                        {
                            if (object.ReferenceEquals(data.GetUser(), hit.transform.gameObject)) continue;
                            if (!Scanner.IsTargetVisible(data.GetUser(), hit.collider.gameObject)) continue;

                            objectsFound.Add(hit.transform.gameObject);
                        }    
                        data.SetTargets(objectsFound);

                        targetAquired = true;
                        break;
                    }
                    else if (EventSystem.current.IsPointerOverGameObject())
                    {
                        playerController.EnableControl(true);
                        playerController.EnableTargetSystem(true);
                        playerController.SetTargeting(false);
                        data.CancelAction();
                        break;
                    }
                }

                yield return null;
            }

            playerController.SetTargeting(false);
            finished();
        }
    }
}