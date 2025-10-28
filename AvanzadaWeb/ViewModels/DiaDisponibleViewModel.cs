namespace AvanzadaWeb.ViewModels
{
    public class DiaDisponibleViewModel
    {
        public string Texto { get; set; } = string.Empty;
        public List<HorarioDisponibleViewModel> Slots { get; set; } = new List<HorarioDisponibleViewModel>();
    }
}