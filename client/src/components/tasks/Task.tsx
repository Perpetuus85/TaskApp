import { Card, CardActionArea } from '@mui/material';
import Typography from '@mui/material/Typography';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import dayjs from "dayjs";

type TaskProps = {
  id: number | string;
  summary: string;
  dueDate?: string;
  onClick?: () => void;
};

export default function Task({ id, summary, dueDate, onClick }: TaskProps) {
  const isOverdue = Boolean(dueDate) && dayjs(dueDate).isBefore(dayjs(), 'day');

  return (
    <Card key={id} variant="outlined" sx={{ padding: '0', width: '100%', maxWidth: '100%', height: '108px', margin: '10px 0', textAlign: 'left' }}>
      <CardActionArea sx={{ position: 'relative', height: '100%' }} onClick={onClick}>
        <Typography variant="body1" component="div" sx={{ position: 'absolute', top: 8, left: 8 }}>
          {summary}
        </Typography>
        {dueDate && (
          <Typography
            variant="caption"
            component="div"
            sx={{
              position: 'absolute',
              right: 8,
              bottom: 8,
              display: 'flex',
              alignItems: 'center',
              gap: 0.5,
              color: isOverdue ? 'error.light' : 'text.secondary',
            }}
          >
            {isOverdue ? <WarningAmberIcon sx={{ fontSize: 14 }} /> : <CalendarMonthIcon sx={{ fontSize: 14 }} />}
            {dayjs(dueDate).local().format('MMM DD, YYYY')}
          </Typography>
        )}
      </CardActionArea>
    </Card>
  );
}
