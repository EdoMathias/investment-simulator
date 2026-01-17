// Generic utilities for making HTTP requests to our server
import type { ApiError } from "./types";

/**
 * 
 * @param url - The URL to make a GET request to
 * @returns The response body as JSON
 */
export async function get<T>(url: string): Promise<T> {
    const response = await fetch(url);
    return handleResponse<T>(response);
}

/**
 * 
 * @param url - The URL to make a POST request to
 * @param body - The body of the request
 * @returns The response body as JSON
 */
export async function post<TRes, TBody>(url: string, body: TBody): Promise<TRes> {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(body),
    });
    return handleResponse<TRes>(response);
}

/**
 * 
 * @param response - The response to parse
 * @returns The response body as JSON
 */
async function handleResponse<T>(response: Response): Promise<T> {
    const text = await response.text();

    // If we get ApiError, throw an error with the code and message
    if (response.ok === false) {
        try {
            const error = JSON.parse(text) as ApiError;
            throw new Error(`${error.code}: ${error.message}`);
        } catch {
            throw new Error(`HTTP ${response.status}: ${text || response.statusText}`);
        }
    }

    // If the response is empty, return undefined
    if (!text) {
        return undefined as T;
    }

    // Parse the response as JSON
    return JSON.parse(text) as T;
}