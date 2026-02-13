package com.polancou.apibasecore.infrastructure.services;

import com.polancou.apibasecore.application.dtos.ExternalAuthUserInfo;
import com.polancou.apibasecore.application.interfaces.IExternalAuthValidator;
import org.springframework.stereotype.Service;

@Service
public class ExternalAuthValidator implements IExternalAuthValidator {

    @Override
    public ExternalAuthUserInfo validateToken(String idToken) {
        // Mock implementation
        if ("VALID_TEST_TOKEN".equals(idToken)) {
            ExternalAuthUserInfo info = new ExternalAuthUserInfo();
            info.setEmail("test@google.com");
            info.setName("Test User");
            info.setProviderSubjectId("123456789");
            info.setPictureUrl("http://example.com/pic.jpg");
            return info;
        }
        return null;
    }
}
