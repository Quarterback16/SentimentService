using System.Collections.Generic;

namespace SentimentService.Source.Models
{
    public class PunditContext
    {
        public string Season { get; set; }
        public List<Pundit> Pundits { get; set; }
    }
}
