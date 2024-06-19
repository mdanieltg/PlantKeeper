export interface WateringLog {
  id: string;
  plantId: string;
  wateringMethodId: string;
  keeperId: string;
  date: Date;
  comments?: string;
}
