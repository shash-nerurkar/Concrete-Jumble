using UnityEngine;

public class Weapon : Hitter
{
    public Entity Entity;

    void FixedUpdate () {
        // if ( Main.IsWeaponMidAir )
        //     transform.rotation = Quaternion.Lerp ( Quaternion.Euler( 0, 0, 0), Quaternion.Euler( 0, 0, 360 ), Entity.Rigidbody.velocity.magnitude * Time.deltaTime );
    }
}
