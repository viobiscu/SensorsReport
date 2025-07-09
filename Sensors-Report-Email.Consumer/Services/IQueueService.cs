using System.Threading.Tasks;

namespace Sensors_Report_Email.Consumer;

public interface IQueueService
{
    /// <summary>
    /// Belirtilen kuyruğu dinlemeye başlar ve gelen her mesaj için bir callback fonksiyonu çalıştırır.
    /// </summary>
    /// <param name="onMessageReceived">Gelen mesajı ve delivery tag'ini işleyecek olan asenkron fonksiyon.</param>
    /// <param name="cancellationToken">İşlemin iptal edilip edilmediğini kontrol eden token.</param>
    Task StartConsumingAsync(Func<string, ulong, Task> onMessageReceived, CancellationToken cancellationToken);

    /// <summary>
    /// Bir mesajın başarıyla işlendiğini onaylar ve kuyruktan siler.
    /// </summary>
    /// <param name="deliveryTag">İşlenen mesajın unique etiketi.</param>
    void AcknowledgeMessage(ulong deliveryTag);

    /// <summary>
    /// Bir mesajın işlenemediğini bildirir.
    /// </summary>
    /// <param name="deliveryTag">İşlenen mesajın unique etiketi.</param>
    /// <param name="requeue">Mesajın tekrar işlenmek üzere kuyruğa geri eklenip eklenmeyeceği.</param>
    void RejectMessage(ulong deliveryTag, bool requeue = false);
}