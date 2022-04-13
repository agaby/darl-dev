namespace Darl.GraphQL.Models.Models.Noda
{
    public class NodaTone
    {
        public double r { get; set; }
        public double g { get; set; }
        public double b { get; set; }
        public double a { get; set; }

        public string ToHex()
        {
            return '#' + (r * 256).ToString("X2") + (g * 256).ToString("X2") + (b * 256).ToString("X2");    
        }
    }
}
