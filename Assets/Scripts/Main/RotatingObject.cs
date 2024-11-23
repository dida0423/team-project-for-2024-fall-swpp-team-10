using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RotatingObject : ObjectManager //coin, item
{
    protected float rotationSpeed = 500.0f;

    protected override void Update()
    {
        base.Update();
        transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0), Space.World);
    }

    public override void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Obstacle"))
        {
            if (transform.position.z > 98)
                Destroy(gameObject);
        }
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
