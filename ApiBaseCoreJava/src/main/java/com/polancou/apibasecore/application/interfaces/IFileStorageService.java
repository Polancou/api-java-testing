package com.polancou.apibasecore.application.interfaces;

import java.io.InputStream;

public interface IFileStorageService {
    String saveFile(InputStream fileStream, String fileName) throws java.io.IOException;
    void deleteFile(String fileRoute);
}
