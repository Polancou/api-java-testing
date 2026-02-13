package com.polancou.apibasecore.application.interfaces;

import com.polancou.apibasecore.domain.models.Usuario;

public interface ITokenService {
    String crearToken(Usuario usuario);
    String generarRefreshToken();
}
