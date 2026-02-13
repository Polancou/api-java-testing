package com.polancou.apibasecore.infrastructure.services;

import com.polancou.apibasecore.application.interfaces.IEmailService;
import jakarta.mail.MessagingException;
import jakarta.mail.internet.MimeMessage;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Primary;
import org.springframework.core.io.Resource;
import org.springframework.core.io.ResourceLoader;
import org.springframework.mail.javamail.JavaMailSender;
import org.springframework.mail.javamail.MimeMessageHelper;
import org.springframework.stereotype.Service;
import org.springframework.util.StreamUtils;

import java.io.IOException;
import java.nio.charset.StandardCharsets;

@Service
@Primary
public class SmtpEmailService implements IEmailService {

    private final JavaMailSender javaMailSender;
    private final ResourceLoader resourceLoader;

    @Value("${spring.mail.username}")
    private String fromEmail; // Assuming username is sender for now, or configurable

    @Value("${spring.mail.properties.mail.from.name:ApiBaseCore}")
    private String fromName;

    public SmtpEmailService(JavaMailSender javaMailSender, ResourceLoader resourceLoader) {
        this.javaMailSender = javaMailSender;
        this.resourceLoader = resourceLoader;
    }

    @Override
    public void sendVerificationEmail(String toEmail, String userName, String verificationLink) {
        String subject = "¡Bienvenido a ApiBaseCore! Confirma tu email";
        String htmlBody = loadTemplate("VerificationEmail.html");
        
        if (htmlBody != null) {
            htmlBody = htmlBody.replace("{{UserName}}", userName)
                               .replace("{{Link}}", verificationLink);
            sendEmailInternal(toEmail, subject, htmlBody);
        }
    }

    @Override
    public void sendPasswordResetEmail(String toEmail, String userName, String resetLink) {
        String subject = "Restablece tu contraseña de ApiBaseCore";
        String htmlBody = loadTemplate("PasswordResetEmail.html");
        
        if (htmlBody != null) {
            htmlBody = htmlBody.replace("{{UserName}}", userName)
                               .replace("{{Link}}", resetLink);
            sendEmailInternal(toEmail, subject, htmlBody);
        }
    }

    @Override
    public void sendOrderConfirmationEmail(String toEmail, String userName, int orderId, java.math.BigDecimal totalAmount) {
        String subject = "Confirmación de Pedido #" + orderId;
        String htmlBody = loadTemplate("OrderConfirmationEmail.html");
        
        if (htmlBody != null) {
            htmlBody = htmlBody.replace("{{UserName}}", userName)
                               .replace("{{OrderId}}", String.valueOf(orderId))
                               .replace("{{TotalAmount}}", totalAmount.toString()); // Simple format
            sendEmailInternal(toEmail, subject, htmlBody);
        }
    }

    private String loadTemplate(String templateName) {
        try {
            Resource resource = resourceLoader.getResource("classpath:templates/email/" + templateName);
            return StreamUtils.copyToString(resource.getInputStream(), StandardCharsets.UTF_8);
        } catch (IOException e) {
            // Log error
            System.err.println("Error loading email template: " + templateName + " - " + e.getMessage());
            return null;
        }
    }

    private void sendEmailInternal(String toEmail, String subject, String htmlBody) {
        try {
            MimeMessage message = javaMailSender.createMimeMessage();
            // Multipart mode for HTML
            MimeMessageHelper helper = new MimeMessageHelper(message, true, "UTF-8");
            
            helper.setFrom(fromEmail); // Can set name too if formatted "Name <email>"
            helper.setTo(toEmail);
            helper.setSubject(subject);
            helper.setText(htmlBody, true); // true = isHtml

            javaMailSender.send(message);
        } catch (MessagingException e) {
            System.err.println("Error sending email: " + e.getMessage());
            throw new RuntimeException("Error sending email", e);
        }
    }
}
