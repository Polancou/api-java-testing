package com.polancou.apibasecore.infrastructure.services;

import com.google.api.client.googleapis.auth.oauth2.GoogleIdToken;
import com.google.api.client.googleapis.auth.oauth2.GoogleIdTokenVerifier;
import com.google.api.client.http.javanet.NetHttpTransport;
import com.google.api.client.json.gson.GsonFactory;
import com.polancou.apibasecore.application.dtos.ExternalAuthUserInfo;
import com.polancou.apibasecore.application.interfaces.IExternalAuthValidator;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.io.IOException;
import java.security.GeneralSecurityException;
import java.util.Collections;

@Service
public class GoogleAuthValidator implements IExternalAuthValidator {

    @Value("${authentication.google.client-id}")
    private String clientId;

    @Override
    public ExternalAuthUserInfo validateToken(String idToken) {
        if (clientId == null || clientId.isBlank()) {
            throw new RuntimeException("Google Client ID not configured.");
        }

        GoogleIdTokenVerifier verifier = new GoogleIdTokenVerifier.Builder(new NetHttpTransport(), new GsonFactory())
                .setAudience(Collections.singletonList(clientId))
                .build();

        try {
            GoogleIdToken googleIdToken = verifier.verify(idToken);
            if (googleIdToken != null) {
                GoogleIdToken.Payload payload = googleIdToken.getPayload();

                ExternalAuthUserInfo userInfo = new ExternalAuthUserInfo();
                userInfo.setProviderSubjectId(payload.getSubject());
                userInfo.setEmail(payload.getEmail());
                userInfo.setName((String) payload.get("name"));
                userInfo.setPictureUrl((String) payload.get("picture"));

                return userInfo;
            }
        } catch (GeneralSecurityException | IOException e) {
            // Log error
            System.err.println("Invalid Google Token: " + e.getMessage());
        }

        return null;
    }
}
