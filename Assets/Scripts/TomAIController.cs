using System.Collections;
using UnityEngine;

public class TomAIController : MonoBehaviour
{
    [Header("Waypoints (set in inspector or use defaults)")]
    public Vector3[] waypoints = new Vector3[] {
        new Vector3(-13f, 8f, 0f),
        new Vector3(-3f, 8f, 0f),
        new Vector3(-3f, 17f, 0f)
    };

    [Header("Movement")]
    public float speed = 2f;
    public float waitTime = 1.0f;
    public bool startOnAwake = true;

    private int index = 0;
    private int direction = 1; // 1 = forward through array, -1 = backward
    private Coroutine routine;
    private Animator animator;
    private Rigidbody2D rb2d;

    void Awake()
    {
        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        transform.position = waypoints[0];

        if (startOnAwake)
            routine = StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        while (true)
        {
            Vector3 target = waypoints[index];

            // move towards target (use Rigidbody2D if present so physics blocks work)
            if (rb2d != null)
            {
                while (Vector2.Distance(rb2d.position, new Vector2(target.x, target.y)) > 0.05f)
                {
                    Vector2 next = Vector2.MoveTowards(rb2d.position, new Vector2(target.x, target.y), speed * Time.deltaTime);
                    Vector2 moveDir = (next - rb2d.position).normalized;
                    rb2d.MovePosition(next);
                    UpdateAnimatorMovement(new Vector3(moveDir.x, moveDir.y, 0f));
                    yield return null;
                }
            }
            else
            {
                while (Vector3.Distance(transform.position, target) > 0.05f)
                {
                    Vector3 dir = (target - transform.position).normalized;
                    Vector3 move = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                    Vector3 moveDir = (move - transform.position).normalized;
                    transform.position = move;
                    UpdateAnimatorMovement(moveDir);
                    yield return null;
                }
            }

            // reached waypoint
            UpdateAnimatorMovement(Vector3.zero);
            yield return new WaitForSeconds(waitTime);

            // advance index; reverse when reaching ends to create back-and-forth loop
            if (index == waypoints.Length - 1) direction = -1;
            else if (index == 0) direction = 1;
            index += direction;
        }
    }

    void UpdateAnimatorMovement(Vector3 moveDir)
    {
        if (animator == null) return;

        bool isMoving = moveDir.sqrMagnitude > 0.001f;
        animator.SetBool("isMoving", isMoving);

        float mx = 0f;
        float my = 0f;

        if (isMoving)
        {
            // emulate PlayerController's discrete direction (no diagonal)
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            {
                mx = Mathf.Sign(moveDir.x);
                my = 0f;
            }
            else
            {
                mx = 0f;
                my = Mathf.Sign(moveDir.y);
            }
        }

        animator.SetFloat("moveX", mx);
        animator.SetFloat("moveY", my);
    }

    // Public controls
    public void StartPatrol()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FollowPath());
    }

    public void StopPatrol()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        UpdateAnimatorMovement(Vector3.zero);
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector3 p = Application.isPlaying ? waypoints[i] : transform.TransformPoint(waypoints[i]);
            Gizmos.DrawSphere(p, 0.12f);
            if (i < waypoints.Length - 1)
                Gizmos.DrawLine(p, Application.isPlaying ? waypoints[i + 1] : transform.TransformPoint(waypoints[i + 1]));
        }
    }
}
