using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class ForceApplier : MonoBehaviour
{
    public float force;
    private Rigidbody r;
    private Rigidbody Rigidbody
    {
        get
        {
            if (r == null)
            {
                r = GetComponent<Rigidbody>();
            }
            return r;
        }
    }


    private void Start()
    {
        Rigidbody.AddRelativeForce(transform.forward * force, ForceMode.Impulse);
    }
}
