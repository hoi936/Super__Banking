export interface CustomerProfile {
    id: string;
    customerCode: string;
    fullName: string;
    dateOfBirth?: string;
    gender?: string;
    phoneNumber?: string;
    address?: string;
    status: string;
}
