import { Injectable } from '@angular/core';

declare const grecaptcha: any;

@Injectable({ providedIn: 'root' })
export class RecaptchaService {

  private readonly SITE_KEY_V3 = '6LcPUfYsAAAAAIBzhV-0uh9kCNceCMO4VLSSe-fA';
  private readonly SITE_KEY_V2 = '6Lc6F-csAAAAALlNHekzk7Kqt3PE-RfA1wNdm06N';

  // ─── V3 (inicio-sesion) ───────────────────────────
  cargarV3() {
    if (document.getElementById('recaptcha-v3-script')) return;

    const script = document.createElement('script');
    script.id = 'recaptcha-v3-script';
    script.src = `https://www.google.com/recaptcha/api.js?render=${this.SITE_KEY_V3}`;
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);
  }

  eliminarV3() {
    document.getElementById('recaptcha-v3-script')?.remove();
    document.querySelector('.grecaptcha-badge')?.remove();
  }

  ejecutarV3(action: string): Promise<string> {
    return new Promise((resolve) => {
      grecaptcha.ready(() => {
        grecaptcha.execute(this.SITE_KEY_V3, { action }).then(resolve);
      });
    });
  }

  // ─── V2 (cuenta) ──────────────────────────────────
  cargarV2(containerId: string) {
    if (document.getElementById('recaptcha-v2-script')) {
      this.renderizarV2(containerId);
      return;
    }

    const script = document.createElement('script');
    script.id = 'recaptcha-v2-script';
    script.src = 'https://www.google.com/recaptcha/api.js';
    script.async = true;
    script.defer = true;
    script.onload = () => this.renderizarV2(containerId);
    document.head.appendChild(script);
  }

  private renderizarV2(containerId: string) {
    grecaptcha.render(containerId, {
      sitekey: this.SITE_KEY_V2,
      theme: 'light',
      size: 'normal'
    });
  }

  obtenerTokenV2(): string {
    return grecaptcha.getResponse();
  }

  eliminarV2() {
    document.getElementById('recaptcha-v2-script')?.remove();
    document.querySelector('.g-recaptcha')?.remove();
  }
}
