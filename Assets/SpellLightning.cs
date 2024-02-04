using System.Collections;
using System.Collections.Generic;
using DigitalRuby.ThunderAndLightning;
using UnityEngine;

public class SpellLightning : MonoBehaviour
{
    public float maxChainRange = 15f;
    public int maxChainLinks = 50;
    public LineRenderer lineRenderer;
    public LightningSpellScript spell;
    private bool spellMouseButtonDown;
    public GameObject spellStart;
    public GameObject spellEnd;
    public float textureScrollSpeed = 0f; //How fast the texture scrolls along the beam, can be negative or positive.
    public float textureLengthScale = 1f;

    private List<Transform> targets = new List<Transform>();

    void Update()
    {
        targets.Clear();
        FindTargets(transform.position, maxChainLinks);
        //BuildLightningChain();
        DrawLightningChain();
    }

    void FindTargets(Vector3 position, int linksLeft)
    {
        if (linksLeft == 0) return;

        Collider[] colliders = Physics.OverlapSphere(position, maxChainRange);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Enemy") && !targets.Contains(collider.transform))
            {
                targets.Add(collider.transform);
                FindTargets(collider.transform.position, linksLeft - 1);
                break;
            }
        }
    }

    void DrawLightningChain()
    {
        lineRenderer.positionCount = targets.Count + 1;
        lineRenderer.SetPosition(0, transform.position);
        spellStart.transform.position = transform.position;
        Vector3 lastPos = transform.position;
        for (int i = 0; i < targets.Count; i++)
        {
            Vector3 currentPos = targets[i].position;
            float distance = Vector3.Distance(lastPos, currentPos);
            lineRenderer.material.mainTextureScale = new Vector2(distance / textureLengthScale, 1); //This sets the scale of the texture so it doesn't look stretched
            lineRenderer.material.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
            lineRenderer.SetPosition(i + 1, targets[i].position);
            lastPos = currentPos;
            if (i == targets.Count - 1)
            {
                spellEnd.transform.position = targets[i].position;
            }
        }
    }

    void BuildLightningChain()
    {
        if (Input.GetButton("Fire1"))
        {
            

            if (spell.SpellStart != null && spell.SpellStart.GetComponent<Rigidbody>() == null)
            {
                spell.SpellStart.transform.position = transform.position;
            }
            if (Input.GetMouseButton(0))
            {
                spellMouseButtonDown = true;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Vector3 rayEnd;
                // send out a ray from the mouse click - if it collides with something we are interested in, alter the ray direction
                if (Physics.Raycast(ray, out hit, spell.MaxDistance, spell.CollisionMask))
                {
                    rayEnd = hit.point;
                }
                else
                {
                    rayEnd = ray.origin + (ray.direction * spell.MaxDistance);
                }
                spell.Direction = (rayEnd - spell.SpellStart.transform.position).normalized;
            }
            else
            {
                spellMouseButtonDown = false;
                spell.Direction = gameObject.transform.forward;
            }
            spell.CastSpell();
        }
        else
        {
            spellMouseButtonDown = false;
            spell.StopSpell();
        }
    }
}
