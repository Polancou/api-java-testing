package com.polancou.apibasecore.application.interfaces;

import com.polancou.apibasecore.application.dtos.ExternalAuthUserInfo;

public interface IExternalAuthValidator {
    ExternalAuthUserInfo validateToken(String idToken);
}
