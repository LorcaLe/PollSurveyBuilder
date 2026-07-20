import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Cell, LabelList, ResponsiveContainer } from 'recharts'

const BAR_COLORS = ['#7c3aed', '#ec4899', '#fb923c', '#14b8a6', '#7c3aed', '#ec4899']

export default function PollBars({ options }) {
  if (options.length === 0) {
    return <p className="helper">No votes yet — be the first to share this poll.</p>
  }

  const data = options.map((o) => ({
    name: o.text.length > 18 ? `${o.text.slice(0, 17)}…` : o.text,
    fullName: o.text,
    votes: o.count,
    percentage: o.percentage,
  }))

  return (
    <ResponsiveContainer width="100%" height={Math.max(180, data.length * 56)}>
      <BarChart
        data={data}
        layout="vertical"
        margin={{ top: 4, right: 48, left: 8, bottom: 4 }}
      >
        <CartesianGrid strokeDasharray="3 3" stroke="#e9e6f4" horizontal={false} />
        <XAxis type="number" stroke="#726d8c" tick={{ fontSize: 12, fill: '#726d8c' }} allowDecimals={false} />
        <YAxis
          type="category"
          dataKey="name"
          width={140}
          stroke="#726d8c"
          tick={{ fontSize: 13, fill: '#1c1830' }}
        />
        <Tooltip
          cursor={{ fill: 'rgba(124,58,237,0.06)' }}
          contentStyle={{ background: '#ffffff', border: '1px solid #e9e6f4', borderRadius: 8, boxShadow: '0 8px 24px rgba(28,24,48,0.08)' }}
          labelStyle={{ color: '#1c1830' }}
          formatter={(value, _name, props) => [`${value} vote(s) · ${props.payload.percentage}%`, props.payload.fullName]}
        />
        <Bar dataKey="votes" radius={[0, 6, 6, 0]} isAnimationActive animationDuration={450}>
          {data.map((_, index) => (
            <Cell key={index} fill={BAR_COLORS[index % BAR_COLORS.length]} />
          ))}
          <LabelList dataKey="votes" position="right" fill="#1c1830" fontSize={12} />
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  )
}