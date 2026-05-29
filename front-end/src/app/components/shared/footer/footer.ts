import { Component } from '@angular/core';

@Component({
  selector: 'ui-footer',
  imports: [],
  template: `
  <footer class="main-footer">
  <div class="footer-container">
    <div class="footer-copyright">
      &copy; 2026 CABS computación
    </div>
    <div class="footer-links">
      <a href="#" class="footer-link">Política de Privacidad</a>
      <a href="#" class="footer-link">Términos y Condiciones</a>
    </div>
  </div>
</footer>
  
  `,
  styleUrl: './footer.css',
})
export class UiFooterComponent {
  
}
