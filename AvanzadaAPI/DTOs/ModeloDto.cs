namespace AvanzadaAPI.DTOs
{
    // DTO para listar modelos incluyendo el nombre de la marca
    public class ModeloDto
    {
        public int IDModelo { get; set; }
        public string Nombre { get; set; }
        public int IDMarca { get; set; }
        public string MarcaNombre { get; set; }
    }
}