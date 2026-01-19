using UnityEngine;

public class Chicken_Controller : MonoBehaviour
{
    
    public void OnMove(InputValue value)
 {
         _moveInput = value.Get<Vector2>();
     }
    
     public void OnLook(InputValue value)
     {
         _lookInput = value.Get<Vector2>();
     }
   
    public void OnAttack(InputValue value)
    {
        // We will add the recoil logic here later
    }
    public void OnJump(InputValue value)
    {
        
    }
}
