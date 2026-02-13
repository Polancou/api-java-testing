package com.polancou.apibasecore.infrastructure.services;

import com.polancou.apibasecore.application.interfaces.IEmailService;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;

@Service
public class EmailService implements IEmailService {

    @Override
    public void sendVerificationEmail(String toEmail, String userName, String verificationLink) {
        System.out.println("----- Mock Email Service -----");
        System.out.println("To: " + toEmail);
        System.out.println("Subject: Verify your email");
        System.out.println("Body: Hello " + userName + ", please verify your email: " + verificationLink);
    }

    @Override
    public void sendPasswordResetEmail(String toEmail, String userName, String resetLink) {
        System.out.println("----- Mock Email Service -----");
        System.out.println("To: " + toEmail);
        System.out.println("Subject: Reset your password");
        System.out.println("Body: Hello " + userName + ", click here to reset password: " + resetLink);
    }

    @Override
    public void sendOrderConfirmationEmail(String toEmail, String userName, int orderId, BigDecimal totalAmount) {
        System.out.println("----- Mock Email Service -----");
        System.out.println("To: " + toEmail);
        System.out.println("Subject: Order Confirmation #" + orderId);
        System.out.println("Body: Hello " + userName + ", your order of $" + totalAmount + " is confirmed.");
    }
}
