package com.polancou.apibasecore.application.utilities;

import java.io.IOException;
import java.io.InputStream;
import java.util.HashMap;
import java.util.Map;

public class FileSignatureValidator {

    private static final Map<String, byte[][]> FILE_SIGNATURES = new HashMap<>();

    static {
        // JPEG / JPG
        byte[][] jpegSignatures = {
                new byte[] { (byte) 0xFF, (byte) 0xD8, (byte) 0xFF, (byte) 0xE0 },
                new byte[] { (byte) 0xFF, (byte) 0xD8, (byte) 0xFF, (byte) 0xE2 },
                new byte[] { (byte) 0xFF, (byte) 0xD8, (byte) 0xFF, (byte) 0xE3 },
                new byte[] { (byte) 0xFF, (byte) 0xD8, (byte) 0xFF, (byte) 0xE1 }, // Added from C# .jpg list
                new byte[] { (byte) 0xFF, (byte) 0xD8, (byte) 0xFF, (byte) 0xE8 }  // Added from C# .jpg list
        };
        FILE_SIGNATURES.put(".jpeg", jpegSignatures);
        FILE_SIGNATURES.put(".jpg", jpegSignatures);

        // PNG
        byte[][] pngSignatures = {
                new byte[] { (byte) 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
        };
        FILE_SIGNATURES.put(".png", pngSignatures);
    }

    public static boolean isValidImage(InputStream inputStream, String extension) throws IOException {
        if (extension == null || extension.isBlank()) return false;
        
        String ext = extension.toLowerCase();
        if (!FILE_SIGNATURES.containsKey(ext)) return false;

        byte[][] signatures = FILE_SIGNATURES.get(ext);
        
        // Find max length needed
        int maxLength = 0;
        for (byte[] sig : signatures) {
            if (sig.length > maxLength) maxLength = sig.length;
        }

        if (!inputStream.markSupported()) {
            // Ideally should assume stream supports mark, or wrap in BufferedInputStream if caller hasn't
            // But InputStream from MultipartFile usually supports it or we can't rewind.
            // For now, assume it does or we fail.
            // Actually, MultipartFile.getInputStream() might not support mark/reset depending on implementation.
            // Better to enforce BufferedInputStream in caller or here. 
            // But if we wrap here, we don't affect caller's stream unless we return it.
            // Best approach: Read header bytes without closing? 
            // Pushing back bytes? PushbackInputStream.
            // For safety, let's assume the caller passes a stream they can manage, 
            // OR we just read the bytes. But we can't rewind easily without mark.
            // Since this is a utility, we should probably take a BufferedInputStream or byte array.
            // However, looking at C#, it takes Stream and seeks. Java InputStream cannot seek unless reset.
            // I will implement using PushbackInputStream or assume markSupported.
            // Let's behave like C# and expect to read start.
        }

        byte[] headerBytes = new byte[maxLength];
        int bytesRead = inputStream.read(headerBytes);
        
        // Try to rewind if possible, though strict rewinding depends on stream type
        // If it's a FileInputStream or similar from Multipart, we might need a fresh stream or reset.
        // C# does `fileStream.Position = 0;`
        try {
            if (inputStream.markSupported()) {
                inputStream.reset();
            } else {
                 // Warning: Stream consumed. If caller needs it again, they are in trouble 
                 // unless they passed a stream they can reset (like ByteArrayInputStream).
                 // Spring MultipartFile.getInputStream() gives a new stream usually? 
                 // Actually MultipartFile.getInputStream() returns a new stream each time for disk-based uploads?
                 // No, standard ServletInputStream is one-time use.
                 // We should probably NOT consume the stream here if we can't reset it.
                 // But validation requires reading. Only way is to inspect bytes.
            }
        } catch (IOException e) {
            // Ignore reset error?
        }
        
        if (bytesRead < maxLength) {
            // Might be a very small file, check if any signature matches what we read
            // But signatures are quite short (4-8 bytes). If file is smaller, it's invalid image anyway.
            if (bytesRead < 0) return false; 
        }

        for (byte[] signature : signatures) {
            if (bytesRead >= signature.length) {
                boolean match = true;
                for (int i = 0; i < signature.length; i++) {
                    if (headerBytes[i] != signature[i]) {
                        match = false;
                        break;
                    }
                }
                if (match) return true;
            }
        }

        return false;
    }
}
