using UnityEngine;
using UnityEngine.InputSystem;

[ RequireComponent ( typeof ( Entity ) ) ]
public class Player : Hittable
{
    public Entity Entity;

    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction attackAction;

    public void EnableActions () {
        moveAction.Enable ();
        attackAction.Enable ();
    }

    public void DisableActions () {
        moveAction.Disable ();
        attackAction.Disable ();
    }


    void Awake () {
        attackAction.performed += ctx => {
            Entity.Throw ( startDirection: moveAction.ReadValue<Vector2> () );
        };
    }

    void Update() {
        Vector3 moveDirection = moveAction.ReadValue<Vector2> ();
        Vector3 nextPoint = transform.position + ( moveDirection * Entity.MoveSpeed * Time.deltaTime );
        
        // Entity.Rigidbody.MovePosition ( new Vector2 ( transform.position.x, Mathf.Clamp ( nextPoint.y, -Main.ScreenHeightHalved, Main.ScreenHeightHalved ) );
        transform.position = new Vector3 ( transform.position.x, Mathf.Clamp ( nextPoint.y, -Main.ScreenHeightHalved, Main.ScreenHeightHalved ), 0 );
    }
}
