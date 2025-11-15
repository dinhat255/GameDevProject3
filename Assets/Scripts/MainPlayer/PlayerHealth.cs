using UnityEngine;
using UnityEngine.UI; // Thêm dòng này nếu bạn muốn dùng thanh máu UI

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f; // Máu tối đa
    private float currentHealth;   // Máu hiện tại

    // (Tùy chọn) Thêm dòng này nếu bạn có thanh máu (Slider)
    // public Slider healthSlider;

    void Start()
    {
        // Bắt đầu game với đầy máu
        currentHealth = maxHealth;
        
        // (Tùy chọn) Cập nhật thanh máu lúc bắt đầu
        // if (healthSlider != null)
        // {
        //     healthSlider.maxValue = maxHealth;
        //     healthSlider.value = currentHealth;
        // }
    }

    // Hàm này sẽ được "EnemyHitbox" gọi
    public void TakeDamage(float damageAmount)
    {
        // Trừ máu
        currentHealth -= damageAmount;

        // In ra Console để test
        Debug.Log("Player Health: " + currentHealth + "/" + maxHealth);

        // (Tùy chọn) Cập nhật thanh máu
        // if (healthSlider != null)
        // {
        //     healthSlider.value = currentHealth;
        // }

        // Kiểm tra nếu Player chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Tạm thời, chúng ta chỉ in ra thông báo
        Debug.LogError("PLAYER ĐÃ CHẾT!");
        
        // (Sau này bạn có thể thêm:
        //  - Chạy animation Player chết
        //  - Hiển thị màn hình Game Over
        //  - Tạm dừng game: Time.timeScale = 0f;
        // )
        
        // Vô hiệu hóa GameObject của Player
        gameObject.SetActive(false);
    }
}