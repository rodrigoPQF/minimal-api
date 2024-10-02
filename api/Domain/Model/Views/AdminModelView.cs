using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.Enums;

namespace minimal_api.Domain.Model.Views
{
    public record AdminModelView
    {
        public string Email { get; set; } = default!;
        public Perfil Perfil { get; set; } = default!;

    }
}