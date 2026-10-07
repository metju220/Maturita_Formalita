using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Nastavení v Unity
    public float moveSpeed;
    public float jumpForce;
    public bool onGround;

    // Komponenty hrace
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Načtení komponent hned na začátku
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Ošetření kdy můžu skákat
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            onGround = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Ground"))
            onGround = false;
    }

    private void FixedUpdate()
    {
        // Vstupy od hráče (A/D/šipky a Mezerník)
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float jumpInput = Input.GetAxisRaw("Jump");

        // Otáčení spritu podle směru chůze (doleva / doprava)
        if (horizontalInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        // Výpočet pohybu (osa X podle chůze, osa Y se bere z gravitace)
        Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        
        // Pokud chci skočit a jsem na zemi, přepiš rychlost Y na skok
        if (jumpInput > 0)
        {
            if (onGround)
                movement.y = jumpForce;
        }

        // Použití pohybu na fyziku hráče
        rb.linearVelocity = movement;
    }
}