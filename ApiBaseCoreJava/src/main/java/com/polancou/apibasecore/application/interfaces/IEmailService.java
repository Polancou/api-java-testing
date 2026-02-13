package com.polancou.apibasecore.application.interfaces;

public interface IEmailService {
    void sendVerificationEmail(String toEmail, String userName, String verificationLink);
    void sendPasswordResetEmail(String toEmail, String userName, String resetLink);
    void sendOrderConfirmationEmail(String toEmail, String userName, int orderId, java.math.BigDecimal totalAmount);
}
