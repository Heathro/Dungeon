using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Control;
using Utils;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Area Targeting", menuName = "Abilities/Targeting/Area", order = 0)]
    public class AreaTargeting : TargetingStrategy
    {
        // CONFIG

        [SerializeField] CursorType cursorType = CursorType.Fire;
        [SerializeField] LayerMask layerMask;
        [SerializeField] float radius = 1f;
        [SerializeField] float tolerance = 0.4f;
        [SerializeField] GameObject targetCirclePrefab;
        [SerializeField] bool selfTargetAvailable = false;

        // CACHE

        GameObject targetCircle;
        ParticleSystem.MainModule mainModule;

        // STATE

        static int groundLayerMask = 6;
        static int vanishedLayerMask = 1 << 8;

        // PUBLIC

        public override void StartTargeting(AbilityData data, Action finished)
        {
            PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
            playerController.SetTargeting(true);
            playerController.StartCoroutine(Targeting(data, playerController, finished));
        }

        // PRIVATE

        IEnumerator Targeting(AbilityData data, PlayerController playerController, Action finished)
        {
            bool leftActionBar = false;

            if (targetCircle == null)
            {
                targetCircle = Instantiate(targetCirclePrefab);
                mainModule = targetCircle.GetComponentInChildren<ParticleSystem>().main;
                mainModule.startSizeMultiplier = (radius + tolerance) * 2;
            }
            else
            {
                targetCircle.SetActive(true);
            }

            while (!data.IsCancelled())
            {
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

                RaycastHit hit;
                if (Physics.Raycast(PlayerController.GetMouseRay(), out hit, 1000, ~vanishedLayerMask))
                {
                    bool possible = true;
                    float distance = Vector3.Distance(playerController.transform.position, hit.point);

                    if (hit.transform.gameObject.layer != groundLayerMask)
                    {
                        playerController.SetCursor(cursorType, false);
                        possible = false;
                        playerController.SetActionPointsDisplay(false, 0, 0, "Wrong Target");
                        targetCircle.SetActive(false);
                    }
                    else if (!Scanner.IsTargetVisible(playerController.gameObject, hit.point, distance))
                    {
                        playerController.SetCursor(cursorType, false);
                        possible = false;
                        playerController.SetActionPointsDisplay(false, distance, 0, "Out of View");
                        targetCircle.SetActive(false);
                    }
                    else if (distance > data.GetDistance())
                    {
                        playerController.SetCursor(cursorType, false);
                        possible = false;
                        playerController.SetActionPointsDisplay(false, distance, 0);
                        targetCircle.SetActive(false);
                    }
                    else
                    {
                        playerController.SetCursor(cursorType, true);
                        possible = true;
                        playerController.SetActionPointsDisplay(true, distance, data.GetCost());
                        targetCircle.SetActive(true);
                    }

                    targetCircle.transform.position = hit.point;

                    if (!EventSystem.current.IsPointerOverGameObject() && possible && Input.GetMouseButtonDown(0))
                    {
                        yield return new WaitUntil(() => Input.GetMouseButton(0));
                        data.SetTargetPoint(hit.point);
                        data.SetTargets(GetObjectsInRadius(data, hit.point));
                        break;
                    }
                    else if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
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

            Cursor.visible = true;
            targetCircle.SetActive(false);
            playerController.SetTargeting(false);
            finished();
        }

        IEnumerable<GameObject> GetObjectsInRadius(AbilityData data, Vector3 point)
        {
            foreach (var hit in Physics.SphereCastAll(point, radius - tolerance, Vector3.up, 0))
            {
                if (!selfTargetAvailable && object.ReferenceEquals(data.GetUser(), hit.collider.gameObject)) continue;
                if (!Scanner.IsTargetVisible(targetCircle, hit.collider.gameObject)) continue;

                yield return hit.collider.gameObject;
            }
        }
    }
}