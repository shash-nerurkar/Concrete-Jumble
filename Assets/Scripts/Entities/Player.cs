using UnityEngine;
using UnityEngine.InputSystem;

[ RequireComponent ( typeof ( Entity ) ) ]
public class Player : Hittable
{
    public Entity Entity;

    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction attackAction;

    private bool IsInfected {
        get {
            return ( Entity.CurrentHeld != null && Entity.HeldPivot.childCount > 0 ) || Entity.CurrentHolder != null; 
        }
    }

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
        Entity.Velocity = moveAction.ReadValue<Vector2> () * Time.deltaTime;
        Vector3 nextPoint = transform.position + Entity.Velocity * Entity.MoveSpeed;
        
        if ( transform.parent == Entity.main.GameEntities.transform )
            transform.position = new Vector3 ( transform.position.x, Mathf.Clamp ( nextPoint.y, -Main.ScreenHeightHalved, Main.ScreenHeightHalved ), 0 );

        if ( IsInfected )
            Main.PlayerHealth -= Time.deltaTime;
        
        Entity.hud.HealthBarOutline.gameObject.SetActive ( IsInfected );

        if ( Main.PlayerHealth <= 0 )
            Entity.hud.OnGameEnded ();
    }
}
