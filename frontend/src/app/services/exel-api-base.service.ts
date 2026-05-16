// frontend/src/app/services/exel-api-base.service.ts
import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class ExelApiBaseService {
    protected baseUrl = '/api-exel';  // Antes: 'https://api01.exeldelnorte.com.mx'

    protected async fetchData<T>(endpoint: string, options?: RequestInit): Promise<T> {
        const url = `${this.baseUrl}/${endpoint}`;
        
        console.log(`📡 Fetching: ${url}`); // Para debugging
        
        const response = await fetch(url, {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...options?.headers
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return response.json();
    }

    protected async get<T>(endpoint: string): Promise<T> {
        return this.fetchData<T>(endpoint);
    }

    protected async post<T>(endpoint: string, body: any): Promise<T> {
        return this.fetchData<T>(endpoint, {
            method: 'POST',
            body: JSON.stringify(body)
        });
    }
}