using Coordinator.Enums;

namespace Coordinator.Models
{
    /// <summary>
    /// Kullanıcıdan gelen talepler doğrultusunda servislerin state bilgileri tutulur
    ///1. aşama servis hazır mı (Prepare)
    ///2. asama transaction başarılı mı (Commit)
    /// </summary>
    public record NodeState(Guid TransactionId)
    {
        public Guid Id { get; set; }
        public Node Node { get; set; }
        public ReadyType IsReady { get; set; }
        public TransactionState TransactionState { get; set; }
    }
}
