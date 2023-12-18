using Coordinator.Enums;
using Coordinator.Models;
using Coordinator.Models.Contexts;
using Coordinator.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services
{
    public class TransactionService(IHttpClientFactory _httpClientFactory, AppDbContext _context) : ITransactionService
    {

        HttpClient _orderHttpClient = _httpClientFactory.CreateClient("OrderAPI");
        HttpClient _stockHttpClient = _httpClientFactory.CreateClient("StockAPI");
        HttpClient _paymentHttpClient = _httpClientFactory.CreateClient("PaymentAPI");


        /// <summary>
        /// transaction id ye karşılık gelen node state'ler sorgulanır
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public async Task<bool> CheckReadyServicesAsync(Guid transactionId)
        {
            return (await _context.NodesState
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync()).TrueForAll(ns => ns.IsReady == ReadyType.Ready);
        }

        /// <summary>
        /// Tüm transaction'lar başarılı mı sonlandı
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public async Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
        {
            return (await _context.NodesState
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync()).TrueForAll(ns => ns.TransactionState == TransactionState.Done);

        }

        public async Task CommitAsync(Guid transationId)
        {
            var transactionNodes = await _context.NodesState
                .Where(ns => ns.TransactionId == transationId)
                .Include(ns => ns.Node)
                .ToListAsync();

            foreach(var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("commit"),
                        "Stock.API" => _stockHttpClient.GetAsync("commit"),
                        "Payment.API" => _paymentHttpClient.GetAsync("commit")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.TransactionState = result ? TransactionState.Done : TransactionState.Abort;
                }
                catch
                {
                    transactionNode.TransactionState = TransactionState.Abort;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<Guid> CreateTransactionAsync()
        {
            Guid transactionId = Guid.NewGuid();
            var nodes = await _context.Nodes.ToListAsync();

            nodes.ForEach(node => node.NodeStates = new List<NodeState>()
            {
                new(transactionId)
                {
                    IsReady = ReadyType.Pending,
                    TransactionState = TransactionState.Pending

                }
            });

            await _context.SaveChangesAsync();
            return transactionId;
        }

        /// <summary>
        /// Transaction id ile ilişkili olan node'lara istek yapılıp 
        /// hazır olup olmadıkları tespit edilir
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public async Task PrepareServicesAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodesState
                .Include(ns => ns.Node)
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    // node'ların hazır olup olmadıklarını anlamak için ilgili node'ların ready
                    // endpointlerine istek yapılır
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("ready"),
                        "Stock.API" => _stockHttpClient.GetAsync("ready"),
                        "Payment.API" => _paymentHttpClient.GetAsync("ready")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.IsReady = result ? ReadyType.Ready : ReadyType.Unready;
                }
                catch
                {
                    transactionNode.IsReady = ReadyType.Unready;
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Transactionlardan bir tane bile başarısız ise
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task RollbackAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodesState
                .Where(ns => ns.TransactionId == transactionId)
                .Include(ns => ns.Node)
                .ToListAsync();

            foreach(var transactionNode in transactionNodes)
            {
                try
                {
                    if (transactionNode.TransactionState == TransactionState.Done)
                    {
                        _ = await (transactionNode.Node.Name switch
                        {
                            "Order.API" => _orderHttpClient.GetAsync("rollback"),
                            "Stock.API" => _stockHttpClient.GetAsync("rollback"),
                            "Payment.API" => _paymentHttpClient.GetAsync("rollback")
                        });
                    }

                    transactionNode.TransactionState = TransactionState.Abort;

                }
                catch
                {
                    transactionNode.TransactionState = TransactionState.Abort;
                }
            }

            await _context.SaveChangesAsync();

        }
    }
}
