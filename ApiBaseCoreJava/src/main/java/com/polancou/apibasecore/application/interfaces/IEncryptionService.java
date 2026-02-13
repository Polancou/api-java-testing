package com.polancou.apibasecore.application.interfaces;

public interface IEncryptionService {
    String encrypt(String plainText);
    String decrypt(String cipherText);
}
