using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyRam : MonoBehaviour
{
    private Transform PrevTaran;
    private Part Parent;
    private void Awake()
    {
        Parent = transform.parent.GetComponent<Part>();
    }

    private bool TaranAiOke(Armor armor)
    {
        return armor != null && Parent.Parent != null && armor.transform.root != transform.root && armor.transform.root != PrevTaran;
    }
    private void Taran(Armor armor)
    {
        if (TaranAiOke(armor) && !GameData.InBuild)
        {
            PrevTaran = armor.transform.root;
            Part EnemyBody = armor.Main;
            Tank enemy = null;
            if (armor.Main != null && armor.Main.Parent != null)
                enemy = armor.Main.Parent;
            Tank self = Parent.Parent;
            float Normal = Mathf.Abs(Vector2.Dot(transform.up, armor.transform.right));
            float RigNormal = Mathf.Abs(Vector2.Dot(transform.up, armor.transform.up));
            Vector2 EnemyVelocity = enemy != null ? enemy.Rig.velocity : Vector2.zero;
            Vector2 SelfVelocity = self != null ? self.Rig.velocity : Vector2.zero;
            float Velocity = (SelfVelocity - EnemyVelocity).magnitude * Normal;
            float MassVs = (self != null ? self.Mass : Parent.Mass) / (enemy != null ? enemy.Mass : armor.Main.Mass);
            int Damage = Mathf.FloorToInt(Mathf.Sqrt(Velocity) * MassVs);
            Vector2 EnemyAddForce = (self != null ? self.Rig.velocity : Vector2.zero) * MassVs;
            if(enemy != null)
            {
                enemy.Rig.velocity += EnemyAddForce * 0.1f;
            }
            else
            {
                Parent.GetHit(Mathf.FloorToInt(Mathf.Sqrt(Velocity) / MassVs), null);
            }
            if(self != null)
            {
                self.Rig.velocity *= ((self.Rig.velocity - EnemyAddForce / MassVs * 0.25f) / (self.Rig.velocity.magnitude + 0.1f)).magnitude;
                self.Power *= ((self.Rig.velocity - EnemyAddForce / MassVs * 0.25f) / (self.Rig.velocity.magnitude + 0.1f)).magnitude;
                self.Rig.velocity -= EnemyAddForce / MassVs * 0.25f;
            }
            Parent.Parent.InOther = MassVs > 1 ? 1 : MassVs * 0.5f;
            armor.Hit(Damage, 1f, transform.position, 0, Vector2.zero, enemy, ExitArmor);
        }
    }
    private void Taran(StaticObject Obj)
    {
        Tank Self = Parent.Parent;
        float Dot = Mathf.Abs(Vector2.Dot(transform.up, (transform.position - Obj.transform.position).normalized));
        float Velocity = Self != null ? Self.Rig.velocity.magnitude : 0;
        float MassVs = (Self != null ? Self.Mass : Parent.Mass) / Obj.Mass;

        int SelfDamage = Mathf.FloorToInt(Dot * Velocity / Mathf.Sqrt(MassVs) / 4);
        int Damage = Mathf.FloorToInt(Dot * Velocity * Mathf.Sqrt(MassVs));
        if(Self != null)
        {
            Self.Rig.velocity *= (1 - Dot * 0.75f / Mathf.Sqrt(MassVs));
            Self.Power *= (1 - Dot * 0.75f / Mathf.Sqrt(MassVs));
        }
        Parent.GetHit(SelfDamage, null);
        Obj.GetHit(Damage, transform.position);
    }
    private void Taran(ObjectPart part)
    {
        Tank Self = Parent.Parent;
        float Dot = Mathf.Abs(Vector2.Dot(transform.up, (transform.position - part.transform.position).normalized));
        float Velocity = Self != null ? Self.Rig.velocity.magnitude : 0;
        float MassVs = (Self != null ? Self.Mass : Parent.Mass) / part.Mass;

        int SelfDamage = Mathf.FloorToInt(Dot * Velocity / Mathf.Sqrt(MassVs) / 4);
        int Damage = Mathf.FloorToInt(Dot * Velocity * Mathf.Sqrt(MassVs));
        if (Self != null)
        {
            Self.Rig.velocity *= (1 - Dot * 0.75f / Mathf.Sqrt(MassVs));
            Self.Power *= (1 - Dot * 0.75f / Mathf.Sqrt(MassVs));
        }
        Parent.GetHit(SelfDamage, null);
        part.GetHit(Damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.root.tag == "Ai")
        {
            Taran(collision.transform.GetComponent<Armor>());
        }
        else if(collision.transform.tag == "Object")
        {
            Taran(collision.transform.GetComponent<StaticObject>());
        }
        else if (collision.transform.tag == "ObjectPart")
        {
            Taran(collision.transform.GetComponent<ObjectPart>());
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.root == PrevTaran)
        {
            PrevTaran = null;
            ExitArmor();
        }
        
    }
    private void ExitArmor()
    {
        if (Parent.Parent != null)
        {
            Parent.Parent.InOther = 0;
        }
    }
}
