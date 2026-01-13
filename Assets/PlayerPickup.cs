using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPosition;
    public float pickupDistance;

    [Header("Pickup Offset")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Throw Settings")]
    public float maxThrowDistance = 10f;
    public LayerMask groundLayer;
    public float arcHeight = 2f;
    public float ignoreCollisionTime = 0.5f;

    [Header("Trajectory Visulization")]
    public LineRenderer trajectoryLine;
    public int trajectoryResolution = 30;
    public GameObject targetIndicator;
    public Transform throwStartPoint;

    [Header("UI & other vital references")]
    public TextMeshProUGUI pickupPromptText;
    [SerializeField] private GameManager gameManager;

    [Header("Inventory")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventoryUI uiInventory;
    [SerializeField] private Transform inventoryStorage;
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private Collider heldPhysicsCollider;
    private Collider heldTriggerCollider;
    private int currentInventoryIndex = -1;

    private InputAction throwAction;
    private InputAction dropAction;
    private InputAction[] numberKeys;

    private Camera mainCamera;
    private bool isAiming = false;
    private Vector3 targetPosition;
    private Vector3 plannedVelocity;
    private float plannedFlightTime;

    void OnEnable()
    {
        dropAction = new InputAction(type: InputActionType.Button);
        dropAction.AddBinding("<Keyboard>/q");
        dropAction.Enable();
        throwAction = new InputAction(type: InputActionType.Button);
        throwAction.AddBinding("<Mouse>/leftButton");
        throwAction.Enable();

        numberKeys = new InputAction[6];
        for(int i = 0; i < 6; i++)
        {
            numberKeys[i] = new InputAction(type:InputActionType.Button);
            numberKeys[i].AddBinding($"<Keyboard>/{i + 1}");
            numberKeys[i].Enable();
        }
    }
    void OnDisable()
    {
        dropAction?.Disable();
        throwAction?.Disable();
    }
    void Awake()
    {
        if(pickupPromptText == null)
        {
            pickupPromptText = gameManager.infoText;
        }
        inventory = new Inventory();
        if(uiInventory != null) uiInventory.SetInventory(inventory);

        if(inventoryStorage == null)
        {
            GameObject storage = new GameObject("InventoryStorage");
            storage.transform.SetParent(transform);
            storage.transform.localPosition = new Vector3(0, -1000, 0);
            inventoryStorage = storage.transform;
        }
    }
    void Start()
    {
        if(pickupPromptText == null)
        {
            pickupPromptText = gameManager.infoText;
        }
        mainCamera = Camera.main;
        pickupPromptText.gameObject.SetActive(false);
        trajectoryLine.enabled = false;
        targetIndicator.SetActive(false);
    }
    public void ShowPickupPrompt(string message)
    {
        if(pickupPromptText != null)
        {
            pickupPromptText.text = message;
            pickupPromptText.gameObject.SetActive(true);
        }
    }
    public void HidePickupPrompt()
    {
        pickupPromptText.gameObject.SetActive(false);
    }
    public void PickupObject(GameObject obj, Collider physicsCollider, Collider triggerCollider)
    {
        if(heldObject != null)
        {
            StoreCurrentItem();
        }
       obj.transform.SetParent(inventoryStorage);
       obj.transform.localPosition = Vector3.zero;
       obj.SetActive(false);

        physicsCollider.enabled = false;
        triggerCollider.enabled = false;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        PickupObject pickupObj = obj.GetComponent<PickupObject>();
        if(pickupObj != null)
        {
            pickupObj.OnPickedUp();

            Item.ItemType itemType = GetItemTypeFromName(pickupObj.itemName);
            Item newItem = new Item
            {
                itemType = itemType,
            };
            newItem.objectInstances.Add(obj);
            newItem.physicsColliders.Add(physicsCollider);
            newItem.triggerColliders.Add(triggerCollider);

            inventory.AddItem(newItem);

            int count = inventory.GetItemCount(itemType);
            Debug.Log($"Picked up {itemType}. New Stack Count: {count}");

            RefreshInventoryIndex(itemType);

            EquipItem(inventory.GetItemList()[currentInventoryIndex], currentInventoryIndex);

            uiInventory.SetInventory(inventory);
            uiInventory.HighlightSlot(currentInventoryIndex);
        }
        HidePickupPrompt();
    }
    private void EquipItem(Item item, int index)
    {
        if(item.objectInstances.Count == 0) return;
        GameObject objToEquip = item.objectInstances[0];
        heldObject = objToEquip;
        heldObjectRb = heldObject.GetComponent<Rigidbody>();
        heldPhysicsCollider = item.physicsColliders[0];
        heldTriggerCollider = item.triggerColliders[0];

        heldObject.SetActive(true);
        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = positionOffset;
        heldObject.transform.localRotation = Quaternion.Euler(rotationOffset);

        if(heldObjectRb != null)
        {
            heldObjectRb.isKinematic = true;
            heldObjectRb.useGravity = false;
        }
        currentInventoryIndex = index;
    }
    private void RefreshInventoryIndex(Item.ItemType type)
    {
        var list = inventory.GetItemList();
        for(int i = 0; i < list.Count; i++)
        {
            if(list[i].itemType == type)
            {
                currentInventoryIndex = i;
                return;
            }
        }
    }
    void Update()
    {
        for(int i = 0; i < numberKeys.Length; i++)
        {
            if (numberKeys[i].WasPressedThisFrame())
            {
                SelectInventorySlot(i);
            }
        }
        if(heldObject != null && dropAction.WasPressedThisFrame())
        {
            DropObject();
        }
        else if (throwAction.WasPressedThisFrame())
        {
            StartAiming();
        }
        else if(throwAction.IsPressed() && isAiming)
        {
            UpdateAiming();
        }
        else if(isAiming && !throwAction.IsPressed())
        {
            ThrowObject();
        }
    }
    private void SelectInventorySlot(int index)
    {
        var itemList = inventory.GetItemList();
        if(index >= itemList.Count)
        {
            return;
        }
        if(index == currentInventoryIndex)
        {
            return;
        }
        if(heldObject != null)
        {
            StoreCurrentItem();
        }
        Item selectedItem = itemList[index];
        EquipItem(selectedItem, index);

        uiInventory.HighlightSlot(index);
    }
    private void StoreCurrentItem()
    {
        if(heldObject == null) return;
        heldObject.transform.SetParent(inventoryStorage);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.SetActive(false);

        heldObject = null;
        heldObjectRb = null;
        heldPhysicsCollider = null;
        heldTriggerCollider = null;
    }
    private void StartAiming()
    {
        if(heldObject == null) return;

        isAiming = true;
        trajectoryLine.enabled = true;
        targetIndicator.SetActive(true);
    }
    private void UpdateAiming()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 1000f, groundLayer))
        {
            Vector3 start = GetLineStartPosition();
            Vector3 toTarget = hit.point - start;
            Vector3 horizontalDirection = new Vector3 (toTarget.x, 0f, toTarget.z);
            float horizontalDistance = horizontalDirection.magnitude;

            if(horizontalDistance > maxThrowDistance)
            {
                horizontalDirection = horizontalDirection.normalized * maxThrowDistance;

                Vector3 clampedWorldPos = new Vector3(start.x + horizontalDirection.x, start.y, start.z + horizontalDirection.z);

                if(Physics.Raycast(clampedWorldPos + Vector3.up * 100f, Vector3.down, out var groundHit, 200f, groundLayer))
                {
                    targetPosition = groundHit.point;
                }
                else
                {
                    targetPosition = new Vector3(clampedWorldPos.x, hit.point.y, clampedWorldPos.z);
                }
            }
            else
            {
                targetPosition = hit.point;
            }
            if(SolveBallisticArcWithApex(start, targetPosition, arcHeight, out var v0, out var flightTime))
            {
                Vector3 predictedImpact = DrawTrajectory(start, v0, flightTime);
                plannedVelocity = v0;
                plannedFlightTime = flightTime;

                if(targetIndicator != null)
                {
                    targetIndicator.transform.position = predictedImpact + Vector3.up * 0.05f;
                }
            }
        }
    }
    private Vector3 GetLineStartPosition()
    {
        if(throwStartPoint != null)
        {
            return throwStartPoint.position;
        }
        return transform.position;
    }
    private Vector3 DrawTrajectory(Vector3 start, Vector3 initialVelocity, float flightTime)
    {
        if(trajectoryLine == null) return start;
        trajectoryLine.positionCount = trajectoryResolution;

        Vector3 prevPoint = start;
        Vector3 hitPoint = start;

        int lastIndex = trajectoryResolution - 1;
        for(int i = 0; i < trajectoryResolution; i++)
        {
            float t = Mathf.Lerp(0f, flightTime, i / (float)lastIndex);
            Vector3 point = CalculatePhysicsPoint(start, initialVelocity, t);
            trajectoryLine.SetPosition(i, point);

            if(i > 0)
            {
                Vector3 segment = point - prevPoint;
                float dist = segment.magnitude;
                if(dist > 0.001f)
                {
                    if(Physics.Raycast(prevPoint, segment.normalized, out var hit, dist, groundLayer))
                    {
                        hitPoint = hit.point;
                        for(int j = i; j < trajectoryResolution; j++)
                        {
                            trajectoryLine.SetPosition(j, hitPoint);
                        }
                        return hitPoint;
                    }
                }
            }
            hitPoint = point;
            prevPoint = point;
        }
        return hitPoint;
    }
    private Vector3 CalculatePhysicsPoint(Vector3 start, Vector3 velocity, float time)
    {
        Vector3 point = start + velocity * time;
        point.y += 0.5f * Physics.gravity.y * time * time;

        return point;
    }
    private bool SolveBallisticArcWithApex(Vector3 start, Vector3 end, float extraApexHeight, out Vector3 velocity, out float totalTime)
    {
        float g = Mathf.Abs(Physics.gravity.y);

        float startY = start.y;
        float endY = end.y;
        float apexY = Mathf.Max(startY, endY) + Mathf.Max(0.01f, extraApexHeight);
        float vy = Mathf.Sqrt(2f * g * (apexY - startY));

        float timeUp = vy / g;
        float timeDownSquared = 2f * Mathf.Max(0f, apexY - endY) / g;
        if(timeDownSquared <= 0f)
        {
            velocity = Vector3.zero;
            totalTime = 0f;
            return false;
        }
        float timeDown = Mathf.Sqrt(timeDownSquared);
        totalTime = timeUp + timeDown;
        Vector3 to = end - start;
        Vector3 toXZ = new Vector3(to.x, 0f, to.z);
        Vector3 vxz = toXZ / Mathf.Max(0.001f, totalTime);

        velocity = vxz + Vector3.up * vy;
        return true;
    }
    private Vector3 CalculateArcPointForTrajectory(Vector3 start, Vector3 end, float t)
    {
        Vector3 velocity = CalculateThrowVelocity(start, end);
        float time = t * CalculateFlightTime(start, end);
        Vector3 point = start + velocity * time;
        point.y += 0.5f * Physics.gravity.y * time * time;
        return point;
    }
    private float CalculateFlightTime(Vector3 start, Vector3 target)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        Vector3 direction = target - start;
        float verticalDistance = direction.y;

        float time = Mathf.Sqrt((2 * arcHeight) / gravity) + Mathf.Sqrt(Mathf.Max(0, (2 * (arcHeight - verticalDistance)) / gravity));
        if(time <= 0) time = 1f;
        return time;
    }
    private Vector3 CalculateThrowVelocity(Vector3 start, Vector3 target)
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        Vector3 direction = target - start;
        float horizontalDistance = new Vector3(direction.x, 0, direction.z).magnitude;
        float verticalDistance = direction.y;
        float time = Mathf.Sqrt((2 * arcHeight) / gravity) + Mathf.Sqrt(Mathf.Max(0, (2 * (arcHeight - verticalDistance)) / gravity));

        if(time <= 0) time = 1f;
        Vector3 velocity = new Vector3(direction.x, 0, direction.z) / time;
        velocity.y = (verticalDistance / time) + (0.5f * gravity * time);
        return velocity;
    }
    private void DropObject()
    {
        if(heldObject == null) return;
        PickupObject pickupObj = heldObject.GetComponent<PickupObject>();
        if(pickupObj != null)
        {
            pickupObj.OnDropped();
        }
        heldObject.transform.SetParent(null);
        if(heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.useGravity = true;
        }
        if(heldPhysicsCollider != null)
        {
            heldPhysicsCollider.enabled = true;
        }
        if(heldTriggerCollider != null)
        {
            heldPhysicsCollider.enabled = true;
        }
        if(heldTriggerCollider != null)
        {
            heldTriggerCollider.enabled = true;
        }
        RemoveCurrentItemFromInventory();
        
    }
    private void ThrowObject()
    {
        if(heldObject == null) return;
        isAiming = false;
        trajectoryLine.enabled = false;
        targetIndicator.SetActive(false);

        PickupObject pickupObj = heldObject.GetComponent<PickupObject>();
        if(pickupObj != null)
        {
            pickupObj.OnThrown();
        }
        Vector3 throwStartPos = GetLineStartPosition();
        heldObject.transform.SetParent(null);
        
        if(heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
            heldObjectRb.useGravity = true;
            heldObjectRb.linearVelocity = plannedVelocity;
        }
        heldPhysicsCollider.enabled = true;
        heldTriggerCollider.enabled = true;

        Collider playerCollider = GetComponent<Collider>();
        if(playerCollider != null && heldPhysicsCollider != null)
        {
            Physics.IgnoreCollision(heldPhysicsCollider, playerCollider, true);
            Collider objectCol = heldPhysicsCollider;
            StartCoroutine(ReenableCollisionAfterDelay(objectCol, playerCollider));
        }
        RemoveCurrentItemFromInventory();
    }
    private void RemoveCurrentItemFromInventory()
    {
        if(currentInventoryIndex < 0) return;
        var itemList = inventory.GetItemList();
        if(currentInventoryIndex < itemList.Count)
        {
            Item item = itemList[currentInventoryIndex];
            inventory.RemoveOneFromStack(item);
            uiInventory.SetInventory(inventory);

            if(item.amount > 0)
            {
                EquipItem(item, currentInventoryIndex);
            }
            else
            {
                heldObject = null;
                currentInventoryIndex = -1;
            }
        }
    }
    private IEnumerator ReenableCollisionAfterDelay(Collider objectCol, Collider playerCol)
    {
        yield return new WaitForSeconds(ignoreCollisionTime);
        if(objectCol != null && playerCol != null)
        {
            Physics.IgnoreCollision(objectCol, playerCol, false);
        }
    }

    private Item.ItemType GetItemTypeFromName(string Name)
    {
        string nameLower = name.ToLower().Trim();

        if(nameLower.Contains("squeaky")) return Item.ItemType.Squeaky;
        if(nameLower.Contains("clock")) return Item.ItemType.Clock;

        return Item.ItemType.Squeaky; // I have nothing for default so like we gonna use this for now
    }
}
