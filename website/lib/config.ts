import config from '../config.json';

export const API_BASE = config.api.baseUrl;

export function getLoginUrl(): string {
    const params = new URLSearchParams({
        client_id: config.oauth.clientId,
        response_type: 'code',
        scope: config.oauth.scopes,
        redirect_uri: config.oauth.redirectUri
    });
    return `${config.oauth.authorizeUrl}?${params.toString()}`;
}

export default config;
