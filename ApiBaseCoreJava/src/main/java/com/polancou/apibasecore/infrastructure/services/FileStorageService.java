package com.polancou.apibasecore.infrastructure.services;

import com.polancou.apibasecore.application.interfaces.IFileStorageService;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;
import org.springframework.util.StringUtils;

import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;

@Service
public class FileStorageService implements IFileStorageService {

    private final Path fileStorageLocation;
    private static final String CONTAINER_NAME = "uploads/avatars";

    public FileStorageService(@Value("${file.upload-dir:./uploads}") String uploadDir) {
        // Base upload dir, then subfolder
        this.fileStorageLocation = Paths.get(uploadDir).toAbsolutePath().normalize();

        try {
            Files.createDirectories(this.fileStorageLocation.resolve("avatars"));
        } catch (Exception ex) {
            throw new RuntimeException("Could not create the directory where the uploaded files will be stored.", ex);
        }
    }

    @Override
    public String saveFile(InputStream inputStream, String fileName) throws IOException {
        String cleanFileName = StringUtils.cleanPath(fileName);
        
        // Check if filename contains invalid characters
        if(cleanFileName.contains("..")) {
            throw new RuntimeException("Sorry! Filename contains invalid path sequence " + cleanFileName);
        }

        // Target location: {uploadDir}/avatars/{fileName}
        Path targetLocation = this.fileStorageLocation.resolve("avatars").resolve(cleanFileName);
        
        Files.copy(inputStream, targetLocation, StandardCopyOption.REPLACE_EXISTING);

        // Return relative URL matching C# logic: /uploads/avatars/filename
        return "/" + CONTAINER_NAME + "/" + cleanFileName;
    }

    @Override
    public void deleteFile(String fileRoute) {
        if (fileRoute == null || fileRoute.isBlank()) return;

        // Convert relative URL to path
        // url: /uploads/avatars/abc.jpg
        // remove leading / and 'uploads/' part if we mapped it weirdly, 
        // but simple logic: we know it is in 'avatars' folder inside our storage root if it follows convention.
        // However, C# logic joined WebRoot + relativePath.
        // Here we have `fileStorageLocation` which is `{root}/uploads`.
        // And requested fileRoute is `/uploads/avatars/file.jpg`.
        
        String cleanRoute = fileRoute.replace("/", java.io.File.separator);
        if (cleanRoute.startsWith(java.io.File.separator)) cleanRoute = cleanRoute.substring(1);
        
        // cleanRoute is "uploads/avatars/file.jpg"
        // storageLocation is ".../uploads"
        
        // Resolving: we need to be careful.
        // If fileRoute contains "uploads/avatars", we should construct path relative to parent of storageLocation?
        // Or simpler: just extract filename if we know structure?
        // Let's replicate C# logic: LocalFileStorageService uses WebRootPath + relativeRoute.
        // Here we defined `fileStorageLocation` as base. 
        // Let's assume `file.upload-dir` is the root where `uploads` folder sits? 
        // defaulted to `./uploads`. 
        // logic: `this.fileStorageLocation` = `./uploads`.
        // `CONTAINER_NAME` = `uploads/avatars`. 
        // Wait, if container is `uploads/avatars`, then `fileStorageLocation` should probably be just `./`.
        // C# : WebRootPath + "uploads/avatars" + fileName.
        
        // Let's just implement robustly:
        // Identify filename from route.
        Path filePath; 
        // We know we store in `fileStorageLocation/avatars/`.
        String fileName = Paths.get(fileRoute).getFileName().toString();
        filePath = this.fileStorageLocation.resolve("avatars").resolve(fileName);

        try {
            Files.deleteIfExists(filePath);
        } catch (IOException e) {
            // Log?
            System.err.println("Could not delete file: " + filePath + " " + e.getMessage());
        }
    }
}
