using AutoMapper;
using GestorCitasAPI.Models;
using GestorCitasAPI.DTOs;

namespace GestorCitasAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Cliente mappings
            CreateMap<Cliente, ClienteDTO>();
            CreateMap<CrearClienteDTO, Cliente>();
            CreateMap<ClienteDTO, Cliente>();

            // Cita mappings
            CreateMap<Cita, CitaDTO>();
            CreateMap<CrearCitaDTO, Cita>();
            CreateMap<Cita, CitaDetalleDTO>()
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente.Nombre))
                .ForMember(dest => dest.ClienteEmail, opt => opt.MapFrom(src => src.Cliente.Email))
                .ForMember(dest => dest.ServicioNombre, opt => opt.MapFrom(src => src.Servicio.Nombre))
                .ForMember(dest => dest.ProfesionalNombre, opt => opt.MapFrom(src => src.Profesional.Nombre));
        }
    }
}