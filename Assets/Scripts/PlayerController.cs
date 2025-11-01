using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;

    private bool isMoving;

    private Vector2 input;

    private Animator animator;

    public LayerMask interactableLayer;

    public LayerMask ObjectsLayer;

    private void Awake()
    {

        animator = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            //Debug.Log("This is input.x" + input.x);
            //Debug.Log("This is input.y" + input.y);

            //damit es nicht diagonal geht
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }

        animator.SetBool("isMoving", isMoving);


        if (Input.GetKeyDown(KeyCode.L))
            Interact();
    }


    void Interact()
    {
        Debug.Log("L gedruckt");
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 5f);

        //Debug.DrawLine(transform.position, transform.position + Vector3.right * 2, Color.red, 5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.2f, interactableLayer | ObjectsLayer);

        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
            Debug.Log("Interagiert mit: " + collider.name);
        }
        else
        {
            Debug.Log("Kein Interactable gefunden!");
        }

    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, interactableLayer | ObjectsLayer) != null)
        {
            return false;
        }
        return true;

    }
}
