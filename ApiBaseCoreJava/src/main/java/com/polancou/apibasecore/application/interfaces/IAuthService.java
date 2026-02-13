package com.polancou.apibasecore.application.interfaces;

import com.polancou.apibasecore.application.dtos.ExternalLoginDto;
import com.polancou.apibasecore.application.dtos.LoginUsuarioDto;
import com.polancou.apibasecore.application.dtos.RegistroUsuarioDto;
import com.polancou.apibasecore.application.dtos.ResetPasswordDto;
import com.polancou.apibasecore.application.dtos.TokenResponseDto;
import com.polancou.apibasecore.domain.models.AuthResult;

public interface IAuthService {
    AuthResult register(RegistroUsuarioDto registroDto);
    TokenResponseDto login(LoginUsuarioDto loginDto);
    TokenResponseDto externalLogin(ExternalLoginDto externalLoginDto);
    TokenResponseDto refreshToken(String refreshToken);
    AuthResult verifyEmail(String token);
    AuthResult forgotPassword(String email);
    AuthResult resetPassword(ResetPasswordDto dto);
}
