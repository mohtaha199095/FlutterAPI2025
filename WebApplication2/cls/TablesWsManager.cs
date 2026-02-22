using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication2.cls
{
    public static class TablesWsManager
    {
        private class Client
        {
            public WebSocket Ws { get; set; } = default!;
            public int BranchId { get; set; }
        }

        private static readonly ConcurrentDictionary<string, Client> _clients = new();

        public static void Add(string id, WebSocket ws)
        {
            _clients[id] = new Client
            {
                Ws = ws,
                BranchId = 0 // يتحدد بعد subscribe
            };
        }

        public static void SetBranch(string id, int branchId)
        {
            if (_clients.TryGetValue(id, out var c))
                c.BranchId = branchId;
        }

        public static async Task Remove(string id)
        {
            if (_clients.TryRemove(id, out var c))
            {
                try
                {
                    if (c.Ws.State == WebSocketState.Open)
                        await c.Ws.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "closed",
                            CancellationToken.None);
                }
                catch { }

                try { c.Ws.Dispose(); } catch { }
            }
        }

        // 🔥 هذا المهم لحالة الطاولات
        public static async Task BroadcastToBranch(int branchId, string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var seg = new ArraySegment<byte>(bytes);

            foreach (var c in _clients.Values)
            {
                if (c.BranchId != branchId) continue;
                if (c.Ws.State != WebSocketState.Open) continue;

                try
                {
                    await c.Ws.SendAsync(seg, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch { }
            }
        }
    }
}
