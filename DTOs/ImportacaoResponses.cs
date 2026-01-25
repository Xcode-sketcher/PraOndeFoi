using System.Collections.Generic;

namespace PraOndeFoi.DTOs
{
    public class ImportacaoResultadoResponse
    {
        public int LinhasProcessadas { get; set; }
        public int LinhasImportadas { get; set; }
        public List<string> Erros { get; set; } = new();
    }
}
