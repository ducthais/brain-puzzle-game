using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class HybridUIButton : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    public enum DragMode { None, Free, VerticalOnly }

    [Header("--- SỰ KIỆN KHI CLICK ---")]
    public UnityEvent onClickEvent;

    [Header("--- CẤU HÌNH CLICK/DRAG ---")]
    public float clickThreshold = 15f;
    public float clickTimeLimit = 0.4f;
    public DragMode dragMode = DragMode.Free;

    [Header("--- VẬT LÝ TƯƠNG TÁC ---")]
    public bool isSolidPlatform = true;
    public string groundLayerName = "Ground";
    public Vector2 colliderSize = new Vector2(1f, 1f);
    public Vector2 colliderOffset = new Vector2(0f, 0f); // Khuyên dùng 0,0 để ở giữa

    private RectTransform _rectTransform;
    private Canvas _parentCanvas;
    private Camera _mainCam;

    private GameObject _physicsProxy;
    private Rigidbody2D _proxyRb; // Thêm Rigidbody để đẩy NVC
    private Vector3 _targetWorldPos; // Lưu vị trí đích

    private Vector2 _pointerDownPosition;
    private float _pointerDownTime;
    private bool _isDragging = false;
    private float _initialX; // Dùng để khóa trục X cho thang máy

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();
        _mainCam = Camera.main;
        _initialX = _rectTransform.anchoredPosition.x;
    }

    void Start()
    {
        if (isSolidPlatform)
        {
            CreatePhysicsProxy();
        }
    }

    private void CreatePhysicsProxy()
    {
        _physicsProxy = new GameObject("PhysicsProxy_" + gameObject.name);

        BoxCollider2D col = _physicsProxy.AddComponent<BoxCollider2D>();
        col.size = colliderSize;
        col.offset = colliderOffset;

        // THÊM RIGIDBODY KINEMATIC ĐỂ NÂNG ĐỠ NVC MƯỢT MÀ, KHÔNG XUYÊN THẤU
        _proxyRb = _physicsProxy.AddComponent<Rigidbody2D>();
        _proxyRb.bodyType = RigidbodyType2D.Kinematic;
        _proxyRb.useFullKinematicContacts = true;
        _proxyRb.interpolation = RigidbodyInterpolation2D.Interpolate;

        int layerIndex = LayerMask.NameToLayer(groundLayerName);
        if (layerIndex != -1) _physicsProxy.layer = layerIndex;

        _physicsProxy.transform.SetParent(null);
    }

    void Update()
    {
        if (isSolidPlatform && _physicsProxy != null)
        {
            // Tính toán vị trí trong Update
            Vector3 screenPos = _rectTransform.position;
            screenPos.z = Mathf.Abs(_mainCam.transform.position.z);
            _targetWorldPos = _mainCam.ScreenToWorldPoint(screenPos);
            _targetWorldPos.z = 0f;
        }
    }

    void FixedUpdate()
    {
        // DI CHUYỂN BẰNG CÁCH GÁN TỌA ĐỘ TRỰC TIẾP THAY VÌ MOVEPOSITION
        if (isSolidPlatform && _proxyRb != null)
        {
            // Sử dụng rb.position giống hệt nút DraggablePhysicalButton ở Level 9
            // Hành động này dịch chuyển khối tàng hình tức thời.
            // Nếu khối tàng hình chèn vào chân NVC, Unity sẽ dùng lực giải quyết chồng chéo 
            // (một lực hằng số, cực êm) để đẩy NVC lên trên mặt khung.
            _proxyRb.position = _targetWorldPos;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDownPosition = eventData.position;
        _pointerDownTime = Time.realtimeSinceStartup;
        _isDragging = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragMode == DragMode.None) return;
        _isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || _parentCanvas == null) return;

        if (dragMode == DragMode.Free)
        {
            _rectTransform.anchoredPosition += eventData.delta / _parentCanvas.scaleFactor;
        }
        else if (dragMode == DragMode.VerticalOnly)
        {
            Vector2 newPos = _rectTransform.anchoredPosition + (eventData.delta / _parentCanvas.scaleFactor);
            newPos.x = _initialX; // Khóa cứng trục X
            _rectTransform.anchoredPosition = newPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float distanceMoved = Vector2.Distance(_pointerDownPosition, eventData.position);
        float timeHeld = Time.realtimeSinceStartup - _pointerDownTime;

        if (distanceMoved < clickThreshold && timeHeld < clickTimeLimit)
        {
            onClickEvent.Invoke();
        }
    }
}