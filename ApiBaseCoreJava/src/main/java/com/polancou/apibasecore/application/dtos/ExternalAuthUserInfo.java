package com.polancou.apibasecore.application.dtos;

import lombok.Data;

@Data
public class ExternalAuthUserInfo {
    private String providerSubjectId;
    private String email;
    private String name;
    private String pictureUrl;
}
