// frontend/src/app/services/exel-api-base.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class ExelApiBaseService {
    protected baseUrl = '/api-exel';

    constructor(protected http: HttpClient) {}

    protected async get<T>(endpoint: string): Promise<T> {
        const url = `${this.baseUrl}/${endpoint}`;
        console.log(`🌐 GET request to: ${url}`);
        
        try {
            const response = await firstValueFrom(this.http.get<T>(url));
            console.log(`✅ Response received:`, response);
            return response;
        } catch (error) {
            console.error(`❌ Error fetching ${url}:`, error);
            throw error;
        }
    }

    protected async post<T>(endpoint: string, body: any): Promise<T> {
        const url = `${this.baseUrl}/${endpoint}`;
        console.log(`🌐 POST request to: ${url}`);
        
        try {
            const response = await firstValueFrom(this.http.post<T>(url, body));
            return response;
        } catch (error) {
            console.error(`❌ Error posting to ${url}:`, error);
            throw error;
        }
    }
}