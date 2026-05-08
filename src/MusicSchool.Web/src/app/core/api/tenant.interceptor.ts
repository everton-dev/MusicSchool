import { HttpInterceptorFn } from '@angular/common/http';

const tenantHeaderName = 'X-Tenant-Id';
const defaultTenantId = '11111111-1111-1111-1111-111111111111';

export const tenantInterceptor: HttpInterceptorFn = (request, next) => {
  const tenantId = localStorage.getItem('music-school-tenant-id') ?? defaultTenantId;

  if (request.headers.has(tenantHeaderName)) {
    return next(request);
  }

  return next(request.clone({
    headers: request.headers.set(tenantHeaderName, tenantId)
  }));
};
