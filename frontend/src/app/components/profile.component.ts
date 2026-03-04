import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../services/account.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.component.html'
})
export class ProfileComponent implements OnInit, OnDestroy {
  @ViewChild('faceVideo') faceVideoRef?: ElementRef<HTMLVideoElement>;
  @ViewChild('faceCanvas') faceCanvasRef?: ElementRef<HTMLCanvasElement>;

  email = 'upi.user@example.com';
  name = 'UPI User';

  phoneNumber = '';
  upiPin = '';
  otpCode = '';
  faceImageBase64 = '';
  cameraError = '';
  captured = false;

  bankDirectory: any[] = [];
  linkedAccounts: any[] = [];

  infoMessage = '';
  errorMessage = '';
  snackbarMessage = '';

  private cameraStream: MediaStream | null = null;

  constructor(private auth: AuthService, private router: Router, private accountService: AccountService) {}

  ngOnInit() {
    const token = this.auth.getToken();
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.email = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || this.email;
      this.name = this.email.split('@')[0];
    }

    this.loadBankDirectory();
    this.loadLinkedAccounts();
    this.startCamera();
  }

  ngOnDestroy() {
    this.stopCamera();
  }

  async startCamera() {
    this.cameraError = '';

    try {
      this.stopCamera();
      this.cameraStream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' }, audio: false });
      if (this.faceVideoRef?.nativeElement) {
        this.faceVideoRef.nativeElement.srcObject = this.cameraStream;
      }
    } catch {
      this.cameraError = 'Camera access denied or unavailable. Please allow camera permission.';
    }
  }

  captureFace() {
    const video = this.faceVideoRef?.nativeElement;
    const canvas = this.faceCanvasRef?.nativeElement;
    if (!video || !canvas || video.videoWidth === 0 || video.videoHeight === 0) {
      this.cameraError = 'Camera stream is not ready yet. Please try again.';
      return;
    }

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    const ctx = canvas.getContext('2d');
    if (!ctx) {
      this.cameraError = 'Unable to capture face frame.';
      return;
    }

    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
    const dataUrl = canvas.toDataURL('image/jpeg', 0.92);
    this.faceImageBase64 = dataUrl.split(',')[1] || '';
    this.captured = !!this.faceImageBase64;
  }

  private stopCamera() {
    if (this.cameraStream) {
      this.cameraStream.getTracks().forEach(track => track.stop());
      this.cameraStream = null;
    }
  }

  loadBankDirectory() {
    this.accountService.getBankDirectory().subscribe((items: any) => {
      this.bankDirectory = items;
    });
  }

  loadLinkedAccounts() {
    this.accountService.getLinkedAccounts().subscribe((items: any) => {
      this.linkedAccounts = items;
    });
  }

  requestOtp() {
    this.errorMessage = '';
    this.infoMessage = '';

    this.accountService.requestOtp(this.phoneNumber, this.upiPin).subscribe({
      next: (response: any) => {
        this.infoMessage = `${response.message}. Demo OTP: ${response.demoOtp}`;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.extractError(error);
      }
    });
  }

  verifyOtpAndLink() {
    this.errorMessage = '';
    this.infoMessage = '';

    if (!this.faceImageBase64) {
      this.errorMessage = 'Live face capture is required to verify OTP and link account';
      return;
    }

    this.accountService.verifyOtp(this.phoneNumber, this.otpCode, this.faceImageBase64).subscribe({
      next: (response: any) => {
        this.infoMessage = `${response.message} | UPI ID: ${response.upiId}`;
        this.showSnackbar(`UPI ID created successfully: ${response.upiId}`);
        this.loadLinkedAccounts();
        this.otpCode = '';
        this.faceImageBase64 = '';
        this.captured = false;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.extractError(error);
      }
    });
  }

  removeLinkedAccount(linkedId: number) {
    this.accountService.removeLinkedAccount(linkedId).subscribe({
      next: () => {
        this.loadLinkedAccounts();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.extractError(error);
      }
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }

  private extractError(error: HttpErrorResponse) {
    return typeof error.error === 'string' ? error.error : error.error?.message || error.error?.detail || 'Something went wrong';
  }

  private showSnackbar(message: string) {
    this.snackbarMessage = message;
    setTimeout(() => {
      this.snackbarMessage = '';
    }, 3000);
  }
}
